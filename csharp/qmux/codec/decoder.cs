namespace qmux.codec;

using System.Text;
using System.Threading;
using gostdlib.errors;
using gostdlib.io;
public class Decoder
{
    private Mutex mutex = new Mutex();
    private io.IReader? r;

    public Decoder(io.IReader r)
    {
        this.r = r;
    }

    public (IMessage?, errors.Error?) Decode()
    {
        this.mutex.WaitOne();
        if (this.r == null)
        {
            System.Environment.FailFast("reader shoudlnt be null");
        }

        var (packet, err) = Marshaler.ReadPacket(this.r);
        if (err != null)
        {
            return (null, err);
        }

        if (Debug.Bytes != null)
        {
            var debugBytes = Encoding.BigEndianUnicode.GetBytes($">>DEC {packet}");
            Debug.Bytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();

        return Marshaler.Decode(packet);
    }
}