namespace qmux.codec;

using System.Text;
using System.Threading;
using gostdlib.errors;
using gostdlib.io;

public class Encoder
{
    private Mutex mutex = new Mutex();
    private io.IWriter w;

    public Encoder(io.IWriter w)
    {
        this.w = w;
    }

    public errors.Error? Encode(object msg)
    {
        this.mutex.WaitOne();
        if (Debug.Messages != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + msg.ToString());
            Debug.Messages.Write(debugBytes);
        }

        var (b, err) = Marshaler.Marshal(msg);
        if (err != null)
        {
            return err;
        }

        var _ = this.w.Write(b);
        if (Debug.Bytes != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + b);
            Debug.Bytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();
        return null;
    }
}