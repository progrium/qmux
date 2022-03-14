using System.Text;
namespace qmux.codec;

using System.Threading;
using gostdlib.errors;
using gostdlib.io;

public class Encoder
{
    private Mutex mutex = new Mutex();
    public IWriter W;

    public Encoder(IWriter w)
    {
        this.W = w;
    }

    public Error? Encode(object msg)
    {
        this.mutex.WaitOne();
        if (Codec.DebugMessages != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + msg.ToString());
            Codec.DebugMessages.Write(debugBytes);
        }

        var (b, err) = Codec.Marshal(msg);
        if (err != null)
        {
            return err;
        }

        var _ = this.W.Write(b);
        if (Codec.DebugBytes != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + b);
            Codec.DebugBytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();
        return null;
    }
}