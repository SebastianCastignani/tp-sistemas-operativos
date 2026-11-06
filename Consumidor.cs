public sealed class Consumidor
{
    private readonly int _id; private readonly BufferAcotado _buffer; private readonly TablaAviones _tabla;
    private readonly Estadisticas _estadisticas; private readonly int _demora;
    public Consumidor(int id, BufferAcotado buffer, TablaAviones tabla, Estadisticas estadisticas, int demora)
        => (_id, _buffer, _tabla, _estadisticas, _demora) = (id, buffer, tabla, estadisticas, demora);
    public void Ejecutar()
    {
        while (true)
        {
            var item = _buffer.Sacar();
            if (ReferenceEquals(item, AvionItem.Fin)) break;
            _tabla.Upsert(item); _estadisticas.Consumido();
            if (_demora > 0) Thread.Sleep(_demora);
        }
        Console.WriteLine($"[C{_id}] terminó");
    }
}
