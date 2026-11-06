public sealed class Consumidor
{
    private readonly BufferAcotado _buffer; private readonly TablaAviones _tabla;
    public Consumidor(BufferAcotado buffer, TablaAviones tabla)
        => (_buffer, _tabla) = (buffer, tabla);
    public void Ejecutar()
    {
        while (true)
        {
            var item = _buffer.Sacar();
            if (ReferenceEquals(item, AvionItem.Fin)) break;
            _tabla.Actualizar(item);
            Interlocked.Increment(ref _buffer.Consumidos);
        }
    }
}
