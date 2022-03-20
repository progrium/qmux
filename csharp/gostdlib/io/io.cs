namespace gostdlib.io;
using gostdlib.errors;
public static partial class io
{
    public interface IWriteCloser : IWriter, ICloser { }
    public interface IReadCloser : IReader, ICloser { }
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

    public static errors.Error EOF = new errors.Error("EOF");
}