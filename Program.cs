const int capacidadBuffer = 8;
const int cantidadConsumidores = 3;
const int cantidadConsultas = 10;

var buffer = new BufferAcotado(capacidadBuffer);
var tabla = new TablaAviones();
var productores = new Thread[3];
var consumidores = new Thread[cantidadConsumidores];

for (var i = 0; i < productores.Length; i++)
{
    if (i == 0)
    {
        productores[i] = new Thread(new ProductorApi(
            "Buenos Aires", -34.6, -58.4, 250, buffer, cantidadConsultas).Ejecutar);
    }
    else if (i == 1)
    {
        productores[i] = new Thread(new ProductorApi(
            "Londres", 51.47, -0.45, 250, buffer, cantidadConsultas).Ejecutar);
    }
    else
    {
        productores[i] = new Thread(new ProductorApi(
            "Nueva York", 40.64, -73.78, 250, buffer, cantidadConsultas).Ejecutar);
    }
}

for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i] = new Thread(new Consumidor(buffer, tabla).Ejecutar);
}

for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i].Start();
}

for (var i = 0; i < productores.Length; i++)
{
    productores[i].Start();
}

for (var i = 0; i < productores.Length; i++)
{
    productores[i].Join();
}

for (var i = 0; i < consumidores.Length; i++)
{
    buffer.Poner(AvionItem.Fin);
}

for (var i = 0; i < consumidores.Length; i++)
{
    consumidores[i].Join();
}

Console.WriteLine($"Producidos: {Interlocked.Read(ref buffer.Producidos)}");
Console.WriteLine($"Consumidos: {Interlocked.Read(ref buffer.Consumidos)}");
Console.WriteLine($"Aviones en la tabla: {tabla.Cantidad}");
