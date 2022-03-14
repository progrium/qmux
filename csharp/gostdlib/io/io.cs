namespace gostdlib.io;

using gostdlib.errors;

public interface IWriter
{
    public int Write(byte[] p);
}

public interface IReader
{
    public (int, Error?) Read(byte[] p);
}

public interface ICloser
{
    public void Close();
}