using System.Net;
using System.Net.Http;
using System.Text.Json;

public sealed class ProductorApi
{
    private readonly Zona _zona; private readonly BufferAcotado _buffer; private readonly Estadisticas _estadisticas;
    private readonly CancellationToken _cancelacion; private readonly int _intervalo; private readonly string _capturas; private readonly Action<string> _log;
    private static long _secuencia;
    public ProductorApi(Zona zona, BufferAcotado buffer, Estadisticas estadisticas, CancellationToken cancelacion, int intervalo, string capturas, Action<string> log)
        => (_zona, _buffer, _estadisticas, _cancelacion, _intervalo, _capturas, _log) = (zona, buffer, estadisticas, cancelacion, Math.Max(3, intervalo), capturas, log);

    public void Ejecutar()
    {
        using var cliente = new HttpClient();
        while (!_cancelacion.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.adsb.lol/v2/lat/{_zona.Lat}/lon/{_zona.Lon}/dist/{_zona.Distancia}";
                using var respuesta = cliente.Send(new HttpRequestMessage(HttpMethod.Get, url));
                if (!respuesta.IsSuccessStatusCode) throw new HttpRequestException($"HTTP {(int)respuesta.StatusCode} {respuesta.StatusCode}");
                var json = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Directory.CreateDirectory(_capturas);
                File.WriteAllText(Path.Combine(_capturas, $"{_zona.Nombre}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.json"), json);
                foreach (var item in TelemetriaParser.Parsear(json, _zona.Nombre, ref _secuencia)) { _buffer.Poner(item); _estadisticas.Producido(); }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or IOException)
            {
                _log($"[API/{_zona.Nombre}] {ex.Message}");
            }
            _cancelacion.WaitHandle.WaitOne(TimeSpan.FromSeconds(_intervalo));
        }
    }
}
