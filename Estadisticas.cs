public sealed class Estadisticas
{
    private long _producidos, _consumidos;
    public long Producidos => Interlocked.Read(ref _producidos);
    public long Consumidos => Interlocked.Read(ref _consumidos);
    public void Producido() => Interlocked.Increment(ref _producidos);
    public void Consumido() => Interlocked.Increment(ref _consumidos);
}
