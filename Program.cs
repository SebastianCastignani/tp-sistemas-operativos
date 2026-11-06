var modo = args.Length > 0 ? args[0].ToLowerInvariant() : "sim"; // api | archivo | sim | mixto
const int DURACION_SEGUNDOS = 15, CAPACIDAD_BUFFER = 8, CANT_CONSUMIDORES = 3;
const int INTERVALO_API_SEGUNDOS = 3, PAUSA_ARCHIVO_MS = 500, PAUSA_SIM_MS = 50, DEMORA_CONSUMIDOR_MS = 300;
const bool RAFAGA_SIMULADA = false;

var zonas = new[]
{
    new Zona("BuenosAires", -34.6, -58.4, 250),
    new Zona("Londres", 51.47, -0.45, 250),
    new Zona("NuevaYork", 40.64, -73.78, 250)
};
var buffer = new BufferAcotado(CAPACIDAD_BUFFER);
var tabla = new TablaAviones();
var estadisticas = new Estadisticas();
using var cancelacion = new CancellationTokenSource();
var productores = new List<Thread>();
var consumidores = Enumerable.Range(1, CANT_CONSUMIDORES)
    .Select(id => new Thread(new Consumidor(id, buffer, tabla, estadisticas, DEMORA_CONSUMIDOR_MS).Ejecutar)).ToList();
var capturas = Path.Combine(AppContext.BaseDirectory, "capturas");

void AgregarProductor(ThreadStart accion) => productores.Add(new Thread(accion));
if (modo is "api" or "mixto")
    foreach (var zona in zonas) AgregarProductor(new ProductorApi(zona, buffer, estadisticas, cancelacion.Token, INTERVALO_API_SEGUNDOS, capturas, Console.WriteLine).Ejecutar);
if (modo == "archivo") AgregarProductor(new ProductorArchivo(capturas, buffer, estadisticas, cancelacion.Token, PAUSA_ARCHIVO_MS, Console.WriteLine).Ejecutar);
if (modo is "sim" or "mixto") AgregarProductor(new ProductorSimulado(buffer, estadisticas, cancelacion.Token, PAUSA_SIM_MS, RAFAGA_SIMULADA).Ejecutar);

var monitor = new Thread(new Monitor(buffer, tabla, estadisticas, cancelacion.Token).Ejecutar);
foreach (var consumidor in consumidores) consumidor.Start();
foreach (var productor in productores) productor.Start();
monitor.Start();
Thread.Sleep(TimeSpan.FromSeconds(DURACION_SEGUNDOS));
cancelacion.Cancel();
foreach (var productor in productores) productor.Join();
for (var i = 0; i < consumidores.Count; i++) buffer.Poner(AvionItem.Fin);
foreach (var consumidor in consumidores) consumidor.Join();
monitor.Join();

tabla.EliminarInactivos(DateTime.UtcNow, TimeSpan.FromSeconds(60));
Console.WriteLine($"\nREPORTE FINAL: producidos={estadisticas.Producidos}, consumidos={estadisticas.Consumidos}, aviones={tabla.Snapshot().Count}, inconsistencias={tabla.VerificarConsistencia()}");
