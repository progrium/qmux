namespace qmux.codec;

using System.Text;
using System.Threading;
using errors = gostdlib.errors;
using io = gostdlib.io;

public class Encoder
{
    private Mutex mutex = new Mutex();
    public io.IWriter w;

    public Encoder(io.IWriter w)
    {
        this.w = w;
    }

    public errors.Error? Encode(object msg)
    {
        this.mutex.WaitOne();
        if (G.DebugMessages != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + msg.ToString());
            G.DebugMessages.Write(debugBytes);
        }

        var (b, err) = G.Marshal(msg);
        if (err != null)
        {
            return err;
        }

        var _ = this.w.Write(b);
        if (G.DebugBytes != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + b);
            G.DebugBytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();
        return null;
    }
}