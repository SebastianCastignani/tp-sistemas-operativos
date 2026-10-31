public class BufferAcotado
{
    private readonly int[] _datos;
    private int _in, _out;
    private readonly SemaphoreSlim _vacios, _llenos;
    private readonly object _lock = new();

    public BufferAcotado(int capacidad)
    {
        _datos = new int[capacidad];
        _vacios = new SemaphoreSlim(capacidad, capacidad);
        _llenos = new SemaphoreSlim(0, capacidad);
    }

    public int Ocupados => _llenos.CurrentCount;

    public void Poner(int item)
    {
        _vacios.Wait();                      // se bloquea si no hay lugar
        lock (_lock)
        {
            _datos[_in] = item;
            _in = (_in + 1) % _datos.Length;
        }
        _llenos.Release();                   // hay un ítem más
    }

    public int Sacar()
    {
        _llenos.Wait();                      // se bloquea si no hay nada
        int item;
        lock (_lock)
        {
            item = _datos[_out];
            _out = (_out + 1) % _datos.Length;
        }
        _vacios.Release();                   // hay un lugar libre
        return item;
    }
}