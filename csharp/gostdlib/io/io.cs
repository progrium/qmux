namespace gostdlib.io;

public interface IWriter
{
    public int Write(byte[] p);
}

public interface IReader
{
    public byte[] Read();
}

public interface ICloser
{
    public void Close();
}