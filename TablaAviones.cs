public sealed class TablaAviones
{
    private readonly Dictionary<string, AvionItem> _aviones = new();
    private readonly object _lock = new();
    private readonly bool _sincronizar;

    public TablaAviones(bool sincronizar = true)
    {
        _sincronizar = sincronizar;
    }

    public void Actualizar(AvionItem item)
    {
        if (_sincronizar)
        {
            // SECCIÓN CRÍTICA: protege el diccionario compartido.
            lock (_lock)
            {
                _aviones[item.Icao] = item;
            }
        }
        else
        {
            _aviones[item.Icao] = item;
        }
    }

    public int Cantidad
    {
        get
        {
            if (_sincronizar)
            {
                // SECCIÓN CRÍTICA: protege la lectura del diccionario compartido.
                lock (_lock)
                {
                    return _aviones.Count;
                }
            }

            return _aviones.Count;
        }
    }
}
