// Hilo consumidor: recibe lecturas del buffer y actualiza la tabla compartida.
// Todos los consumidores usan los mismos objetos buffer y tabla, pero cada hilo tiene su propio stack.
public sealed class Consumidor
{
    // Referencias al buffer y a la tabla que viven en el heap compartido.
    private readonly BufferAcotado _buffer; private readonly TablaAviones _tabla;
    // Guarda las referencias recibidas para que el hilo pueda trabajar con los recursos compartidos.
    public Consumidor(BufferAcotado buffer, TablaAviones tabla)
        => (_buffer, _tabla) = (buffer, tabla);

    // Ejecuta el ciclo del consumidor hasta recibir el mensaje Fin.
    // Es llamado por el hilo consumidor creado en Program.cs.
    public void Ejecutar()
    {
        while (true)
        {
            // Si no hay ítems, Sacar bloquea este hilo en el semáforo de llenos.
            var item = _buffer.Sacar();
            // Fin es un mensaje de control: permite salir del while sin una variable global de cancelación.
            if (ReferenceEquals(item, AvionItem.Fin)) break;
            // La tabla usa su propio lock para evitar carreras entre consumidores.
            _tabla.Actualizar(item);
            // Muestra los datos principales de la lectura que este consumidor procesó.
            Console.WriteLine($"Avión {item.Icao} | Región: {item.Zona} | Vuelo: {item.Callsign} | Altitud: {item.AltitudFt?.ToString() ?? "N/D"} ft | Posición: ({item.Lat:F4}, {item.Lon:F4})");
            // Interlocked hace el incremento atómico; ++ podría perder actualizaciones concurrentes.
            Interlocked.Increment(ref _buffer.Consumidos);
        }
    }
}
