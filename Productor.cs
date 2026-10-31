public class Productor
{
    private readonly int _id;
    private readonly BufferAcotado _buffer;
    private readonly int _cantidad;

    public Productor(int id, BufferAcotado buffer, int cantidad)
    {
        _id = id;
        _buffer = buffer;
        _cantidad = cantidad;
    }

    public void Ejecutar()
    {
        for (int i = 0; i < _cantidad; i++)
        {
            int item = _id * 1000 + i;       // P1 → 1000, 1001... P2 → 2000, 2001...
            Console.WriteLine($"[P{_id}] quiere poner {item} (ocupados={_buffer.Ocupados})");
            _buffer.Poner(item);
            Console.WriteLine($"[P{_id}] puso {item}");
        }
    }
}