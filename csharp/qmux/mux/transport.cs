namespace qmux.mux;

using io = gostdlib.io;

public static partial class mux
{
    public interface ITransport : io.IReader, io.IWriter, io.ICloser { }
}