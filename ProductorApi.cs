using System.Text.Json;

public sealed record Zona(string Nombre, double Lat, double Lon, int Distancia);

public sealed class ProductorApi
{
    private readonly Zona _zona;
    private readonly BufferAcotado _buffer;
    private readonly CancellationToken _cancelacion;
    private readonly int _intervalo;
    private readonly Action<string> _log;
    private static long _secuencia;

    public ProductorApi(Zona zona, BufferAcotado buffer, CancellationToken cancelacion, int intervalo, Action<string> log)
    {
        _zona = zona;
        _buffer = buffer;
        _cancelacion = cancelacion;
        _intervalo = Math.Max(3, intervalo);
        _log = log;
    }

    public void Ejecutar()
    {
        using var cliente = new HttpClient();
        while (!_cancelacion.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.adsb.lol/v2/lat/{_zona.Lat}/lon/{_zona.Lon}/dist/{_zona.Distancia}";
                using var respuesta = cliente.Send(new HttpRequestMessage(HttpMethod.Get, url));
                if (!respuesta.IsSuccessStatusCode)
                    throw new HttpRequestException($"HTTP {(int)respuesta.StatusCode} {respuesta.StatusCode}");

                var json = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                foreach (var item in Parsear(json, _zona.Nombre))
                {
                    _buffer.Poner(item);
                    Interlocked.Increment(ref _buffer.Producidos);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                _log($"[API/{_zona.Nombre}] {ex.Message}");
            }

            _cancelacion.WaitHandle.WaitOne(TimeSpan.FromSeconds(_intervalo));
        }
    }

    private static List<AvionItem> Parsear(string json, string zona)
    {
        var resultado = new List<AvionItem>();
        using var documento = JsonDocument.Parse(json);
        if (!documento.RootElement.TryGetProperty("ac", out var aviones) ||
            aviones.ValueKind != JsonValueKind.Array)
            return resultado;

        foreach (var avion in aviones.EnumerateArray())
        {
            if (!avion.TryGetProperty("hex", out var hex) ||
                !avion.TryGetProperty("lat", out var lat) ||
                !avion.TryGetProperty("lon", out var lon) ||
                lat.ValueKind != JsonValueKind.Number ||
                lon.ValueKind != JsonValueKind.Number)
                continue;

            int? altitud = null;
            if (avion.TryGetProperty("alt_baro", out var altitudJson))
            {
                altitud = altitudJson.ValueKind == JsonValueKind.String &&
                    altitudJson.GetString() == "ground" ? 0 :
                    altitudJson.ValueKind == JsonValueKind.Number &&
                    altitudJson.TryGetInt32(out var pies) ? pies : null;
            }

            resultado.Add(new AvionItem
            {
                Icao = hex.GetString() ?? "",
                Callsign = avion.TryGetProperty("flight", out var flight)
                    ? (flight.GetString() ?? "").Trim() : "",
                AltitudFt = altitud,
                VelocidadKt = avion.TryGetProperty("gs", out var velocidad) &&
                    velocidad.ValueKind == JsonValueKind.Number ? (int)velocidad.GetDouble() : 0,
                Lat = lat.GetDouble(),
                Lon = lon.GetDouble(),
                Squawk = avion.TryGetProperty("squawk", out var squawk) ? squawk.ToString() : "",
                Zona = zona,
                Secuencia = Interlocked.Increment(ref _secuencia),
                TimestampUtc = DateTime.UtcNow
            });
        }

        return resultado;
    }
}
