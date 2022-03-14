namespace qmux.mux;

using gostdlib.io;

public static partial class mux
{
    public interface ITransport : io.IReader, io.IWriter, io.ICloser { }
}