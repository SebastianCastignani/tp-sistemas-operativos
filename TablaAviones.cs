// Recurso compartido donde los consumidores dejan la última lectura de cada avión.
// Su diccionario y su lectura están protegidos por un lock propio, separado del buffer.
public sealed class TablaAviones
{
    // Diccionario compartido: la clave es ICAO y el valor es la lectura más reciente.
    private readonly Dictionary<string, AvionItem> _aviones = new();
    // Mutex de la tabla; evita carreras entre consumidores y el reporte final.
    private readonly object _lock = new();
    // Permite ejecutar una versión sin sincronización para experimentos controlados.
    private readonly bool _sincronizar;

    // Recibe la opción de sincronización y la conserva para las operaciones posteriores.
    public TablaAviones(bool sincronizar = true)
    {
        _sincronizar = sincronizar;
    }

    // Guarda la última lectura del avión; lo llama cada hilo consumidor.
    public void Actualizar(AvionItem item)
    {
        if (_sincronizar)
        {
            // Sin lock, dos consumidores podrían escribir o leer el diccionario a la vez.
            // SECCIÓN CRÍTICA: protege el diccionario compartido.
            lock (_lock)
            {
                _aviones[item.Icao] = item;
            }
        }
        else
        {
            _aviones[item.Icao] = item;
        }
    }

    // Devuelve cuántos aviones distintos conoce la tabla para el reporte final.
    public int Cantidad
    {
        get
        {
            if (_sincronizar)
            {
                // Sin lock, el reporte podría leer el diccionario mientras otro hilo lo modifica.
                // SECCIÓN CRÍTICA: protege la lectura del diccionario compartido.
                lock (_lock)
                {
                    return _aviones.Count;
                }
            }

            return _aviones.Count;
        }
    }
}
