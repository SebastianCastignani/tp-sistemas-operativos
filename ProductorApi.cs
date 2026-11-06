using System.Text.Json;

public sealed class ProductorApi
{
    private readonly string _nombre;
    private readonly double _latitud;
    private readonly double _longitud;
    private readonly int _radio;
    private readonly BufferAcotado _buffer;
    private readonly int _cantidadConsultas;
    private static long _secuencia;

    public ProductorApi(
        string nombre,
        double latitud,
        double longitud,
        int radio,
        BufferAcotado buffer,
        int cantidadConsultas = 10)
    {
        _nombre = nombre;
        _latitud = latitud;
        _longitud = longitud;
        _radio = radio;
        _buffer = buffer;
        _cantidadConsultas = cantidadConsultas;
    }

    public void Ejecutar()
    {
        using var cliente = new HttpClient();

        for (var consulta = 0; consulta < _cantidadConsultas; consulta++)
        {
            try
            {
                var url = $"https://api.adsb.lol/v2/lat/{_latitud}/lon/{_longitud}/dist/{_radio}";
                using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
                using var respuesta = cliente.Send(solicitud);
                respuesta.EnsureSuccessStatusCode();

                var json = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                foreach (var item in Parsear(json))
                {
                    _buffer.Poner(item);
                    Interlocked.Increment(ref _buffer.Producidos);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                Console.WriteLine($"[API/{_nombre}] {ex.Message}");
            }

            if (consulta < _cantidadConsultas - 1)
            {
                Thread.Sleep(5000);
            }
        }
    }

    private List<AvionItem> Parsear(string json)
    {
        var resultado = new List<AvionItem>();
        using var documento = JsonDocument.Parse(json);

        if (!documento.RootElement.TryGetProperty("ac", out var aviones))
        {
            return resultado;
        }

        foreach (var avion in aviones.EnumerateArray())
        {
            if (!avion.TryGetProperty("hex", out var hex) ||
                !avion.TryGetProperty("lat", out var latitud) ||
                !avion.TryGetProperty("lon", out var longitud) ||
                latitud.ValueKind != JsonValueKind.Number ||
                longitud.ValueKind != JsonValueKind.Number)
            {
                continue;
            }

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
                Callsign = avion.TryGetProperty("flight", out var callsign)
                    ? callsign.GetString() ?? "" : "",
                AltitudFt = altitud,
                VelocidadKt = avion.TryGetProperty("gs", out var velocidad) &&
                    velocidad.ValueKind == JsonValueKind.Number
                    ? (int)velocidad.GetDouble() : 0,
                Lat = latitud.GetDouble(),
                Lon = longitud.GetDouble(),
                Squawk = avion.TryGetProperty("squawk", out var squawk)
                    ? squawk.ToString() : "",
                Zona = _nombre,
                Secuencia = Interlocked.Increment(ref _secuencia),
                TimestampUtc = DateTime.UtcNow
            });
        }

        return resultado;
    }
}
