var modo = args.Length > 0 ? args[0].ToLowerInvariant() : "sim"; // api | sim
const int DURACION_SEGUNDOS = 15;
const int CAPACIDAD_BUFFER = 8;
const int CANT_CONSUMIDORES = 3;
const int INTERVALO_API_SEGUNDOS = 3;
const int PAUSA_SIM_MS = 50;
const int DEMORA_CONSUMIDOR_MS = 300;

var zonas = new[]
{
    new Zona("BuenosAires", -34.6, -58.4, 250),
    new Zona("Londres", 51.47, -0.45, 250),
    new Zona("NuevaYork", 40.64, -73.78, 250)
};

var buffer = new BufferAcotado(CAPACIDAD_BUFFER);
var tabla = new TablaAviones();
using var cancelacion = new CancellationTokenSource();
var productores = new List<Thread>();
var consumidores = Enumerable.Range(1, CANT_CONSUMIDORES)
    .Select(id => new Thread(new Consumidor(id, buffer, tabla, DEMORA_CONSUMIDOR_MS).Ejecutar))
    .ToList();

if (modo == "api")
{
    foreach (var zona in zonas)
    {
        var productor = new ProductorApi(zona, buffer, cancelacion.Token,
            INTERVALO_API_SEGUNDOS, Console.WriteLine);
        productores.Add(new Thread(productor.Ejecutar));
    }
}
else
{
    var productor = new ProductorSimulado(buffer, cancelacion.Token, PAUSA_SIM_MS);
    productores.Add(new Thread(productor.Ejecutar));
}

foreach (var consumidor in consumidores) consumidor.Start();
foreach (var productor in productores) productor.Start();

var fin = DateTime.UtcNow.AddSeconds(DURACION_SEGUNDOS);
while (DateTime.UtcNow < fin)
{
    if (cancelacion.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2))) break;
    Console.WriteLine($"[MONITOR] buffer={buffer.Ocupados} producidos={Interlocked.Read(ref buffer.Producidos)} " +
        $"consumidos={Interlocked.Read(ref buffer.Consumidos)} bloqueosP={Interlocked.Read(ref buffer.BloqueosProductores)} " +
        $"bloqueosC={Interlocked.Read(ref buffer.BloqueosConsumidores)} aviones={tabla.Snapshot().Count}");
}

cancelacion.Cancel();
foreach (var productor in productores) productor.Join();
for (var i = 0; i < consumidores.Count; i++) buffer.Poner(AvionItem.Fin);
foreach (var consumidor in consumidores) consumidor.Join();

Console.WriteLine($"\nREPORTE FINAL: producidos={Interlocked.Read(ref buffer.Producidos)}, " +
    $"consumidos={Interlocked.Read(ref buffer.Consumidos)}, aviones={tabla.Snapshot().Count}, " +
    $"inconsistencias={tabla.VerificarConsistencia()}");
