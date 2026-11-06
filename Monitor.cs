public sealed class Monitor
{
    private readonly BufferAcotado _buffer; private readonly TablaAviones _tabla; private readonly Estadisticas _estadisticas;
    private readonly CancellationToken _cancelacion;
    public Monitor(BufferAcotado buffer, TablaAviones tabla, Estadisticas estadisticas, CancellationToken cancelacion)
        => (_buffer, _tabla, _estadisticas, _cancelacion) = (buffer, tabla, estadisticas, cancelacion);
    public void Ejecutar()
    {
        while (!_cancelacion.WaitHandle.WaitOne(2000))
        {
            _tabla.EliminarInactivos(DateTime.UtcNow, TimeSpan.FromSeconds(60));
            Console.WriteLine($"[MONITOR] buffer={_buffer.Ocupados} producidos={_estadisticas.Producidos} consumidos={_estadisticas.Consumidos} bloqueosP={_buffer.BloqueosProductores} bloqueosC={_buffer.BloqueosConsumidores} aviones={_tabla.Snapshot().Count}");
        }
    }
}
