using System.Text.Json;

public sealed class ProductorArchivo
{
    private readonly string _carpeta; private readonly BufferAcotado _buffer; private readonly Estadisticas _estadisticas;
    private readonly CancellationToken _cancelacion; private readonly int _pausa; private readonly Action<string> _log;
    private static long _secuencia;
    public ProductorArchivo(string carpeta, BufferAcotado buffer, Estadisticas estadisticas, CancellationToken cancelacion, int pausa, Action<string> log)
        => (_carpeta, _buffer, _estadisticas, _cancelacion, _pausa, _log) = (carpeta, buffer, estadisticas, cancelacion, pausa, log);
    public void Ejecutar()
    {
        try
        {
            var archivos = Directory.Exists(_carpeta) ? Directory.GetFiles(_carpeta, "*.json").OrderBy(x => x) : Enumerable.Empty<string>();
            foreach (var archivo in archivos)
            {
                if (_cancelacion.IsCancellationRequested) break;
                foreach (var item in TelemetriaParser.Parsear(File.ReadAllText(archivo), Path.GetFileNameWithoutExtension(archivo).Split('_')[0], ref _secuencia))
                { _buffer.Poner(item); _estadisticas.Producido(); }
                if (_cancelacion.WaitHandle.WaitOne(_pausa)) break;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException) { _log($"[ARCHIVO] {ex.Message}"); }
    }
}
