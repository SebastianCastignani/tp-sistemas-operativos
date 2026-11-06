public sealed class BufferAcotado
{
    private readonly AvionItem?[] _datos;
    private int _in, _out, _ocupados;
    private readonly SemaphoreSlim? _vacios, _llenos;
    private readonly object _lock = new();
    private readonly bool _sincronizar;

    public long Producidos;
    public long Consumidos;
    public long BloqueosProductores;
    public long BloqueosConsumidores;

    public BufferAcotado(int capacidad, bool sincronizar = true)
    {
        if (capacidad <= 0) throw new ArgumentOutOfRangeException(nameof(capacidad));
        _datos = new AvionItem[capacidad];
        _sincronizar = sincronizar;
        if (sincronizar)
        {
            _vacios = new SemaphoreSlim(capacidad, capacidad);
            _llenos = new SemaphoreSlim(0, capacidad);
        }
    }

    public int Ocupados => Volatile.Read(ref _ocupados);
    public void Poner(AvionItem item)
    {
        if (_sincronizar)
        {
            if (!_vacios!.Wait(0))
            {
                Interlocked.Increment(ref BloqueosProductores);
                _vacios.Wait();
            }
            // El semáforo se espera antes del lock para no retener el mutex mientras se espera lugar.
            // SECCIÓN CRÍTICA: protege los índices y la escritura del buffer circular.
            lock (_lock)
            {
                _datos[_in] = item;
                _in = (_in + 1) % _datos.Length;
                Interlocked.Increment(ref _ocupados);
            }
            _llenos!.Release();
            return;
        }

        _datos[_in] = item;
        _in = (_in + 1) % _datos.Length;
        _ocupados = Math.Min(_ocupados + 1, _datos.Length);
    }

    public AvionItem Sacar()
    {
        if (_sincronizar)
        {
            if (!_llenos!.Wait(0))
            {
                Interlocked.Increment(ref BloqueosConsumidores);
                _llenos.Wait();
            }
            AvionItem item;
            // El semáforo se espera antes del lock para no retener el mutex mientras se espera un ítem.
            // SECCIÓN CRÍTICA: protege los índices y la lectura del buffer circular.
            lock (_lock)
            {
                item = _datos[_out]!;
                _datos[_out] = null;
                _out = (_out + 1) % _datos.Length;
                Interlocked.Decrement(ref _ocupados);
            }
            _vacios!.Release();
            return item;
        }

        var resultado = _datos[_out]!;
        _datos[_out] = null;
        _out = (_out + 1) % _datos.Length;
        _ocupados = Math.Max(0, _ocupados - 1);
        return resultado;
    }
}
