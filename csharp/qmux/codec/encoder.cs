namespace qmux.codec;

using System.Text;
using System.Threading;
using errors = gostdlib.errors;
using io = gostdlib.io;

public static partial class codec
{
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
            if (codec.DebugMessages != null)
            {
                var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + msg.ToString());
                codec.DebugMessages.Write(debugBytes);
            }

            var (b, err) = codec.Marshal(msg);
            if (err != null)
            {
                return err;
            }

            var _ = this.w.Write(b);
            if (codec.DebugBytes != null)
            {
                var debugBytes = Encoding.UTF8.GetBytes("<<ENC" + b);
                codec.DebugBytes.Write(debugBytes);
            }

            this.mutex.ReleaseMutex();
            return null;
        }
    }
}