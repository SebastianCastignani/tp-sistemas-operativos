// Representa una lectura de un avión producida por la API.
// Es un dato inmutable que viaja del productor al consumidor por el buffer.
public sealed record AvionItem
{
    // Identifica de forma única al avión dentro de la tabla compartida.
    public string Icao { get; init; } = "";
    // Guarda la identificación visible del vuelo, sin espacios sobrantes.
    private string _callsign = "";
    public string Callsign { get => _callsign; init => _callsign = (value ?? "").Trim(); }
    // La altitud puede faltar; cero representa que el avión está en tierra.
    public int? AltitudFt { get; init; }
    // Velocidad del avión expresada en nudos.
    public int VelocidadKt { get; init; }
    // Coordenadas geográficas de la lectura.
    public double Lat { get; init; }
    public double Lon { get; init; }
    // Código transpondedor informado por el avión.
    public string Squawk { get; init; } = "";
    // Zona desde la que el productor obtuvo la lectura.
    public string Zona { get; init; } = "";
    // Número creciente que permite distinguir el orden de los mensajes.
    public long Secuencia { get; init; }
    // Momento UTC en que se creó la lectura.
    public DateTime TimestampUtc { get; init; }
    // Mensaje especial que indica a un consumidor que ya puede terminar.
    public static AvionItem Fin { get; } = new() { Icao = "__FIN__" };
}
