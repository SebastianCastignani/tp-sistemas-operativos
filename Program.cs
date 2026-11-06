// Configuración simple del experimento: capacidad, consumidores y consultas por productor.
const int capacidadBuffer = 5;
const int cantidadConsumidores = 3;
const int cantidadConsultas = 10;

// Estos son los objetos compartidos del heap: todos los hilos reciben las mismas referencias.
// Cada hilo conserva su propio stack, pero buffer y tabla son únicos para todo el proceso.
var buffer = new BufferAcotado(capacidadBuffer);
var tabla = new TablaAviones();
// Los arreglos permiten crear y esperar hilos con for simples, sin LINQ.
var productores = new Thread[3];
var consumidores = new Thread[cantidadConsumidores];

// Se crea un productor por zona; todavía no está ejecutando porque falta llamar a Start.
for (var i = 0; i < productores.Length; i++)
{
    if (i == 0)
    {
        productores[i] = new Thread(new ProductorApi(
            "Buenos Aires", -34.6, -58.4, 150, buffer, cantidadConsultas).Ejecutar);
    }
    else if (i == 1)
    {
        productores[i] = new Thread(new ProductorApi(
            "Londres", 51.47, -0.45, 150, buffer, cantidadConsultas).Ejecutar);
    }
    else
    {
        productores[i] = new Thread(new ProductorApi(
            "Nueva York", 40.64, -73.78, 150, buffer, cantidadConsultas).Ejecutar);
    }
}

// Se crean los consumidores con referencias a los mismos recursos compartidos.
for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i] = new Thread(new Consumidor(buffer, tabla).Ejecutar);
}

// Start pasa cada consumidor del estado creado/listo al estado ejecutando.
for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i].Start();
}

// Start inicia los productores; pueden quedar bloqueados si el buffer se llena.
for (var i = 0; i < productores.Length; i++)
{
    productores[i].Start();
}

// Join espera a que todos los productores terminen sus consultas y no produzcan más datos.
for (var i = 0; i < productores.Length; i++)
{
    productores[i].Join();
}

// Se coloca un Fin por consumidor para que cada uno pueda salir de su while.
for (var i = 0; i < consumidores.Length; i++)
{
    buffer.Poner(AvionItem.Fin);
}

// Join espera a que los consumidores procesen lo pendiente y terminen.
for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i].Join();
}

// El reporte se ejecuta cuando todos los hilos terminaron y muestra el resultado compartido.
Console.WriteLine($"Producidos: {Interlocked.Read(ref buffer.Producidos)}");
Console.WriteLine($"Consumidos: {Interlocked.Read(ref buffer.Consumidos)}");
Console.WriteLine($"Aviones en la tabla: {tabla.Cantidad}");
