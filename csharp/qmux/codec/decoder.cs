namespace qmux.codec;

using System.Text;
using System.Threading;
using gostdlib.errors;
using gostdlib.io;

public static partial class codec
{
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
                System.Environment.FailFast("reader shouldnt be null");
            }

            var (packet, err) = codec.ReadPacket(this.r);
            if (err != null)
            {
                return (null, err);
            }

            if (codec.DebugBytes != null)
            {
                var debugBytes = Encoding.BigEndianUnicode.GetBytes($">>DEC {packet}");
                codec.DebugBytes.Write(debugBytes);
            }

            this.mutex.ReleaseMutex();

            // @stevemurr: is this confusing to call Codec.Decode inside Decode?
            // @stevemurr: is the cost less than the gains from the namespace static class as global holder pattern?
            return codec.Decode(packet);
        }
    }
}