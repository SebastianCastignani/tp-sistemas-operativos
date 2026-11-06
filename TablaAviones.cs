public sealed class TablaAviones
{
    private readonly Dictionary<string, EstadoAvion> _aviones = new();
    private readonly object _lock = new();
    private readonly bool _sincronizar;

    public TablaAviones(bool sincronizar = true) => _sincronizar = sincronizar;

    public void Upsert(AvionItem item)
    {
        if (_sincronizar)
        {
            // SECCIÓN CRÍTICA: protege el diccionario y la actualización campo por campo.
            lock (_lock) { Actualizar(item); }
            return;
        }
        Actualizar(item);
    }

    private void Actualizar(AvionItem item)
    {
        if (!_aviones.TryGetValue(item.Icao, out var estado))
        {
            estado = new EstadoAvion { Icao = item.Icao };
            _aviones[item.Icao] = estado;
        }
        estado.Callsign = item.Callsign;
        estado.AltitudFt = item.AltitudFt;
        estado.SecAltitud = item.Secuencia;
        estado.Lat = item.Lat;
        estado.Lon = item.Lon;
        estado.SecPosicion = item.Secuencia;
        estado.Squawk = item.Squawk;
        estado.Mensajes++;
        estado.UltimoTimestampUtc = item.TimestampUtc;
    }

    public int VerificarConsistencia()
    {
        if (_sincronizar) lock (_lock) // SECCIÓN CRÍTICA: protege la lectura consistente del diccionario.
            return _aviones.Values.Count(a => a.SecAltitud != a.SecPosicion);
        return _aviones.Values.Count(a => a.SecAltitud != a.SecPosicion);
    }

    public void EliminarInactivos(DateTime ahoraUtc, TimeSpan maximo)
    {
        if (_sincronizar) { lock (_lock) Eliminar(); } else Eliminar();
        void Eliminar()
        {
            foreach (var clave in _aviones.Where(p => ahoraUtc - p.Value.UltimoTimestampUtc > maximo).Select(p => p.Key).ToList())
                _aviones.Remove(clave);
        }
    }

    public List<EstadoAvion> Snapshot()
    {
        if (_sincronizar) lock (_lock) // SECCIÓN CRÍTICA: protege la copia de estados mientras se actualiza la tabla.
            return _aviones.Values.Select(a => a.Copia()).ToList();
        return _aviones.Values.Select(a => a.Copia()).ToList();
    }
}
