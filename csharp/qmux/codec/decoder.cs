namespace qmux.codec;

using System.Text;
using System.Threading;
using errors = gostdlib.errors;
using io = gostdlib.io;

public class Decoder
{
    private Mutex mutex = new Mutex();
    private io.IReader? r;

    public Decoder(io.IReader r)
    {
        this.r = r;
    }

    public (Message?, errors.Error?) Decode()
    {
        this.mutex.WaitOne();
        if (this.r == null)
        {
            System.Environment.FailFast("reader shouldnt be null");
        }

        var (packet, err) = G.ReadPacket(this.r);
        if (err != null)
        {
            return (null, err);
        }

        if (G.DebugBytes != null)
        {
            var debugBytes = Encoding.BigEndianUnicode.GetBytes($">>DEC {packet}");
            G.DebugBytes.Write(debugBytes);
        }

        this.mutex.ReleaseMutex();

        // @stevemurr: is this confusing to call Codec.Decode inside Decode?
        // @stevemurr: is the cost less than the gains from the namespace static class as global holder pattern?
        return G.Decode(packet);
    }
}