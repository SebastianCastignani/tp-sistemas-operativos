public sealed record AvionItem
{
    public string Icao { get; init; } = "";
    private string _callsign = "";
    public string Callsign { get => _callsign; init => _callsign = (value ?? "").Trim(); }
    public int? AltitudFt { get; init; }
    public int VelocidadKt { get; init; }
    public double Lat { get; init; }
    public double Lon { get; init; }
    public string Squawk { get; init; } = "";
    public string Zona { get; init; } = "";
    public long Secuencia { get; init; }
    public DateTime TimestampUtc { get; init; }
    public static AvionItem Fin { get; } = new() { Icao = "__FIN__" };
}
