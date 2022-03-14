using System.Text;
namespace qmux.codec;

using System.Threading;
using gostdlib.errors;
using gostdlib.io;

public class Decoder
{
    private Mutex mutex = new Mutex();
    public IReader? R;

    public Decoder(IReader r)
    {
        this.R = r;
    }

    public (Message?, Error?) Decode()
    {
        this.mutex.WaitOne();
        if (this.R == null)
        {
            System.Environment.FailFast("reader shouldnt be null");
        }

        var (packet, err) = Codec.ReadPacket(this.R);
        if (err != null)
        {
            return (null, err);
        }

        if (Codec.DebugBytes != null)
        {
            var debugBytes = Encoding.BigEndianUnicode.GetBytes($">>DEC {packet}");
            Codec.DebugBytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();

        // @stevemurr: is this confusing to call Codec.Decode inside Decode?
        // @stevemurr: is the cost less than the gains from the namespace static class as global holder pattern?
        return Codec.Decode(packet);
    }
}