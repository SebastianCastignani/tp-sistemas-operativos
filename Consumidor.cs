public class Consumidor
{
    public const int FIN = -1;               // señal para que el consumidor termine

    private readonly int _id;
    private readonly BufferAcotado _buffer;
    private readonly int _demoraMs;

    public Consumidor(int id, BufferAcotado buffer, int demoraMs)
    {
        _id = id;
        _buffer = buffer;
        _demoraMs = demoraMs;
    }

    public void Ejecutar()
    {
        while (true)
        {
            int item = _buffer.Sacar();
            if (item == FIN) break;
            Console.WriteLine($"    [C{_id}] sacó {item}");
            Thread.Sleep(_demoraMs);         // consumidor lento a propósito
        }
        Console.WriteLine($"    [C{_id}] terminó");
    }
}