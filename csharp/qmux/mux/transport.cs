namespace qmux.mux;

using gostdlib.io;

public interface ITransport : io.IReader, io.IWriter, io.ICloser { }