using System.Text.Json;

public static class TelemetriaParser
{
    public static List<AvionItem> Parsear(string json, string zona, ref long secuencia)
    {
        var resultado = new List<AvionItem>();
        using var documento = JsonDocument.Parse(json);
        if (!documento.RootElement.TryGetProperty("ac", out var aviones) || aviones.ValueKind != JsonValueKind.Array)
            return resultado;
        foreach (var avion in aviones.EnumerateArray())
        {
            if (!avion.TryGetProperty("hex", out var hex) || !avion.TryGetProperty("lat", out var lat) ||
                !avion.TryGetProperty("lon", out var lon) || lat.ValueKind != JsonValueKind.Number ||
                lon.ValueKind != JsonValueKind.Number)
                continue;
            int? altitud = null;
            if (avion.TryGetProperty("alt_baro", out var alt))
                altitud = alt.ValueKind == JsonValueKind.String && alt.GetString() == "ground" ? 0 :
                    alt.ValueKind == JsonValueKind.Number && alt.TryGetInt32(out var pies) ? pies : null;
            resultado.Add(new AvionItem
            {
                Icao = hex.GetString() ?? "",
                Callsign = avion.TryGetProperty("flight", out var flight) ? (flight.GetString() ?? "").Trim() : "",
                AltitudFt = altitud,
                VelocidadKt = avion.TryGetProperty("gs", out var gs) && gs.ValueKind == JsonValueKind.Number ? (int)gs.GetDouble() : 0,
                Lat = lat.GetDouble(), Lon = lon.GetDouble(),
                Squawk = avion.TryGetProperty("squawk", out var squawk) ? squawk.ToString() : "",
                Zona = zona, Secuencia = Interlocked.Increment(ref secuencia), TimestampUtc = DateTime.UtcNow
            });
        }
        return resultado;
    }
}
