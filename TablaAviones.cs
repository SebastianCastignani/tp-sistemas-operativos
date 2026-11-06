public sealed class TablaAviones
{
    public sealed class EstadoAvion
    {
        public string Icao = "";
        public string Callsign = "";
        public int? AltitudFt;
        public double Lat;
        public double Lon;
        public string Squawk = "";
        public long Mensajes;
        public long SecAltitud;
        public long SecPosicion;

        public EstadoAvion Copia() => (EstadoAvion)MemberwiseClone();
    }

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
    }

    public int VerificarConsistencia()
    {
        if (_sincronizar) lock (_lock) // SECCIÓN CRÍTICA: protege la lectura consistente del diccionario.
            return _aviones.Values.Count(a => a.SecAltitud != a.SecPosicion);
        return _aviones.Values.Count(a => a.SecAltitud != a.SecPosicion);
    }

    public List<EstadoAvion> Snapshot()
    {
        if (_sincronizar) lock (_lock) // SECCIÓN CRÍTICA: protege la copia de estados mientras se actualiza la tabla.
            return _aviones.Values.Select(a => a.Copia()).ToList();
        return _aviones.Values.Select(a => a.Copia()).ToList();
    }
}
