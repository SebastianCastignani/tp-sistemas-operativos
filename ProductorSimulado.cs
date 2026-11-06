public sealed class ProductorSimulado
{
    private readonly BufferAcotado _buffer; private readonly CancellationToken _cancelacion;
    private readonly int _pausa; private readonly Random _random = new(); private static long _secuencia;
    private readonly string[] _pool = Enumerable.Range(0, 50).Select(i => $"icao{i:X6}").ToArray();
    public ProductorSimulado(BufferAcotado buffer, CancellationToken cancelacion, int pausa)
        => (_buffer, _cancelacion, _pausa) = (buffer, cancelacion, Math.Max(0, pausa));
    public void Ejecutar()
    {
        while (!_cancelacion.IsCancellationRequested)
        {
            var item = new AvionItem { Icao = _pool[_random.Next(_pool.Length)], Callsign = "SIM" + _random.Next(1000), AltitudFt = _random.Next(0, 40000), VelocidadKt = _random.Next(100, 500), Lat = -35 + _random.NextDouble(), Lon = -59 + _random.NextDouble(), Zona = "Simulada", Squawk = _random.Next(1000, 7777).ToString(), Secuencia = Interlocked.Increment(ref _secuencia), TimestampUtc = DateTime.UtcNow };
            _buffer.Poner(item); Interlocked.Increment(ref _buffer.Producidos);
            if (_cancelacion.WaitHandle.WaitOne(_pausa)) break;
        }
    }
}
