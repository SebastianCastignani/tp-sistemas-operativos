const int NUM_PRODUCTORES = 2;
const int NUM_CONSUMIDORES = 3;
const int ITEMS_POR_PRODUCTOR = 10;
const int CAPACIDAD = 5;

var buffer = new BufferAcotado(CAPACIDAD);
var hilosP = new List<Thread>();
var hilosC = new List<Thread>();

for (int i = 1; i <= NUM_PRODUCTORES; i++)
    hilosP.Add(new Thread(new Productor(i, buffer, ITEMS_POR_PRODUCTOR).Ejecutar));

for (int i = 1; i <= NUM_CONSUMIDORES; i++)
    hilosC.Add(new Thread(new Consumidor(i, buffer, 300).Ejecutar));

foreach (var h in hilosP) h.Start();
foreach (var h in hilosC) h.Start();

foreach (var h in hilosP) h.Join();          // esperamos que los productores terminen

for (int i = 0; i < NUM_CONSUMIDORES; i++)   // un FIN por consumidor
    buffer.Poner(Consumidor.FIN);

foreach (var h in hilosC) h.Join();
Console.WriteLine("Listo.");