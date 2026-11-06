// Recurso compartido que coordina productores y consumidores con capacidad limitada.
// Usa semáforos para contar espacios e ítems, y un lock para proteger el arreglo circular.
public sealed class BufferAcotado
{
    // Arreglo circular donde se almacenan temporalmente las lecturas.
    private readonly AvionItem?[] _datos;
    // Índices de entrada y salida, y cantidad actual de elementos del buffer.
    private int _in, _out, _ocupados;
    // _vacios cuenta lugares libres; _llenos cuenta ítems listos para consumir.
    private readonly SemaphoreSlim? _vacios, _llenos;
    // Mutex que evita que dos hilos modifiquen índices o datos al mismo tiempo.
    private readonly object _lock = new();
    // Permite desactivar la sincronización para experimentos, aunque el programa usa true.
    private readonly bool _sincronizar;

    // Guarda la cantidad total de lecturas producidas y consumidas.
    public long Producidos;
    public long Consumidos;

    // Recibe la capacidad y decide si esta instancia debe aplicar sincronización.
    // El hilo que construye el buffer inicializa los semáforos con sus conteos iniciales.
    public BufferAcotado(int capacidad, bool sincronizar = true)
    {
        if (capacidad <= 0) throw new ArgumentOutOfRangeException(nameof(capacidad));
        _datos = new AvionItem[capacidad];
        _sincronizar = sincronizar;
        if (sincronizar)
        {
            // Al comenzar hay tantos lugares libres como capacidad y ningún ítem disponible.
            _vacios = new SemaphoreSlim(capacidad, capacidad);
            _llenos = new SemaphoreSlim(0, capacidad);
        }
    }

    // Permite observar la ocupación sin modificar el contador compartido.
    public int Ocupados => Volatile.Read(ref _ocupados);

    // Inserta una lectura; el productor puede quedar bloqueado si no hay lugares libres.
    public void Poner(AvionItem item)
    {
        if (_sincronizar)
        {
            // Wait consume un lugar libre; si el semáforo vale cero, el hilo pasa a bloqueado.
            if (!_vacios!.Wait(0))
            {
                Console.WriteLine("[BUFFER LLENO] El productor queda bloqueado hasta que haya un lugar libre.");
                _vacios.Wait();
            }
            // Wait va antes del lock: así el hilo bloqueado no retiene el mutex y se evita un deadlock.
            // SECCIÓN CRÍTICA: protege los índices y la escritura del buffer circular.
            lock (_lock)
            {
                _datos[_in] = item;
                _in = (_in + 1) % _datos.Length;
                Interlocked.Increment(ref _ocupados);
            }
            // Release devuelve al conteo un ítem disponible para algún consumidor.
            _llenos!.Release();
            return;
        }

        _datos[_in] = item;
        _in = (_in + 1) % _datos.Length;
        _ocupados = Math.Min(_ocupados + 1, _datos.Length);
    }

    // Extrae una lectura; el consumidor puede quedar bloqueado si el buffer está vacío.
    public AvionItem Sacar()
    {
        if (_sincronizar)
        {
            // Wait consume un ítem disponible; con cero ítems el hilo queda bloqueado.
            _llenos!.Wait();
            AvionItem item;
            // Wait va antes del lock: así el hilo bloqueado no retiene el mutex y se evita un deadlock.
            // SECCIÓN CRÍTICA: protege los índices y la lectura del buffer circular.
            lock (_lock)
            {
                item = _datos[_out]!;
                _datos[_out] = null;
                _out = (_out + 1) % _datos.Length;
                Interlocked.Decrement(ref _ocupados);
            }
            // Release anuncia que quedó un lugar libre para otro productor.
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
