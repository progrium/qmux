namespace gostdlib.io;
using gostdlib.errors;
public static partial class io
{
    public interface IWriter
    {
        public int Write(byte[] p);
    }

    public interface IReader
    {
        public (int, errors.Error?) Read(byte[] p);
    }

    public interface ICloser
    {
        public void Close();
    }
}