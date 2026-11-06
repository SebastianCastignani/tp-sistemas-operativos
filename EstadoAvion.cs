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
    public DateTime UltimoTimestampUtc;

    public EstadoAvion Copia() => (EstadoAvion)MemberwiseClone();
}
