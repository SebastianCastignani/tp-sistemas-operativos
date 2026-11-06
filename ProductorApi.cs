using System.Text.Json;
using System.Globalization;

// Hilo productor que consulta una zona de adsb.lol y convierte sus respuestas en lecturas.
// Cada instancia trabaja con una zona y deposita sus resultados en el buffer compartido.
public sealed class ProductorApi
{
    // Datos de la zona usados para construir la URL de consulta.
    private readonly string _nombre;
    private readonly double _latitud;
    private readonly double _longitud;
    private readonly int _radio;
    // Buffer compartido donde se entregan las lecturas a los consumidores.
    private readonly BufferAcotado _buffer;
    // Cantidad fija de consultas que realiza este productor antes de terminar.
    private readonly int _cantidadConsultas;
    // Secuencia común a todos los productores para numerar mensajes sin repetir valores.
    private static long _secuencia;

    // Recibe la zona, el buffer y la cantidad de consultas del hilo productor.
    public ProductorApi(
        string nombre,
        double latitud,
        double longitud,
        int radio,
        BufferAcotado buffer,
        int cantidadConsultas = 10)
    {
        _nombre = nombre;
        _latitud = latitud;
        _longitud = longitud;
        _radio = radio;
        _buffer = buffer;
        _cantidadConsultas = cantidadConsultas;
    }

    // Ejecuta las consultas síncronas; lo llama el Thread correspondiente a la zona.
    // Al finalizar, el hilo queda terminado y Program.cs puede hacer Join.
    public void Ejecutar()
    {
        // El cliente se reutiliza durante todas las consultas de este productor.
        using var cliente = new HttpClient();

        // El productor repite un número conocido de veces y luego termina solo.
        for (var consulta = 0; consulta < _cantidadConsultas; consulta++)
        {
            try
            {
                // La URL identifica coordenadas y radio de la zona consultada.
                var latTexto = _latitud.ToString(CultureInfo.InvariantCulture);  // culture info hace que un string se vuelva formato reginal ejemplo 6,14 pasa a 6.14
                var lonTexto = _longitud.ToString(CultureInfo.InvariantCulture);
                var url = $"https://api.adsb.lol/v2/lat/{latTexto}/lon/{lonTexto}/dist/{_radio}";
                using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
                // Send es síncrono: este hilo queda ejecutando o esperando la respuesta de red.
                using var respuesta = cliente.Send(solicitud);
                // Un estado HTTP incorrecto se transforma en error y se informa sin abortar el proceso.
                respuesta.EnsureSuccessStatusCode();

                // La respuesta se lee de forma síncrona antes de analizar su JSON.
                var json = respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                // Cada avión válido se pone en el buffer; si está lleno, este hilo queda bloqueado.
                foreach (var item in Parsear(json))
                {
                    _buffer.Poner(item);
                    // El incremento atómico evita perder conteos cuando hay varios productores.
                    Interlocked.Increment(ref _buffer.Producidos);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                // Un error de red o formato se registra y el productor continúa con la próxima consulta.
                Console.WriteLine($"[API/{_nombre}] {ex.Message}");
            }

            if (consulta < _cantidadConsultas - 1)
            {
                // Pausa la ejecución del hilo entre consultas para no consultar continuamente la API.
                Thread.Sleep(10000);
            }
        }
    }

    // Convierte el arreglo "ac" de JSON en objetos AvionItem listos para el buffer.
    // Solo se incluyen aviones que tienen ICAO, latitud y longitud utilizables.
    private List<AvionItem> Parsear(string json)
    {
        // Lista temporal que acumula las lecturas de una respuesta.
        var resultado = new List<AvionItem>();
        // JsonDocument permite recorrer la respuesta sin agregar paquetes externos.
        using var documento = JsonDocument.Parse(json);

        // Una respuesta sin "ac" no aporta aviones para producir.
        if (!documento.RootElement.TryGetProperty("ac", out var aviones))
        {
            return resultado;
        }

        // Cada elemento del arreglo representa una aeronave informada por la API.
        foreach (var avion in aviones.EnumerateArray())
        {
            if (!avion.TryGetProperty("hex", out var hex) ||
                !avion.TryGetProperty("lat", out var latitud) ||
                !avion.TryGetProperty("lon", out var longitud) ||
                latitud.ValueKind != JsonValueKind.Number ||
                longitud.ValueKind != JsonValueKind.Number)
            {
                // Sin posición no se puede asociar correctamente la lectura a una ubicación.
                continue;
            }

            // La API puede informar altitud numérica, "ground" o no informar el campo.
            int? altitud = null;
            if (avion.TryGetProperty("alt_baro", out var altitudJson))
            {
                altitud = altitudJson.ValueKind == JsonValueKind.String &&
                    altitudJson.GetString() == "ground" ? 0 :
                    altitudJson.ValueKind == JsonValueKind.Number &&
                    altitudJson.TryGetInt32(out var pies) ? pies : null;
            }

            // Se crea una lectura completa para que el consumidor solo tenga que actualizar la tabla.
            resultado.Add(new AvionItem
            {
                Icao = hex.GetString() ?? "",
                Callsign = avion.TryGetProperty("flight", out var callsign)
                    ? callsign.GetString() ?? "" : "",
                AltitudFt = altitud,
                VelocidadKt = avion.TryGetProperty("gs", out var velocidad) &&
                    velocidad.ValueKind == JsonValueKind.Number
                    ? (int)velocidad.GetDouble() : 0,
                Lat = latitud.GetDouble(),
                Lon = longitud.GetDouble(),
                Squawk = avion.TryGetProperty("squawk", out var squawk)
                    ? squawk.ToString() : "",
                Zona = _nombre,
                // Interlocked asigna una secuencia única aun cuando varios productores ejecutan a la vez.
                Secuencia = Interlocked.Increment(ref _secuencia),
                TimestampUtc = DateTime.UtcNow
            });
        }

        return resultado;
    }
}
