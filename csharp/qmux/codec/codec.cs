namespace qmux.codec;

using System.Text;
using gostdlib.errors;
using gostdlib.io;

public static partial class codec
{
    internal static class ErrorMessages
    {
        public static string MarshalObjectFailed = "marshal object failed";
        public static string UnmarshalObjectFailed = "unmarshal object failed";
        public static string ObjectAsMarshalerFailed = "object v failed to cast to type Marshaler";
        public static string UnmarshalUnsupportedField = "unmarshal unsupported filed";
        public static string DecodeFailedByteArrayEmpty = "decode failed: byte array is empty: expected MessageType in first byte";
        public static string UnexpectedMessageType = "unexpected MessageType passed to Decode";
    }
    public interface IMarshaler
    {
        public (byte[], errors.Error?) MarshalMux();
    }

    public interface IUnmarshaler
    {
        public errors.Error? UnmarshalMux(byte[] b);
    }
    public static io.IWriter? DebugMessages;
    public static io.IWriter? DebugBytes;
    public static (byte[], errors.Error?) Marshal(object v)
    {
        if (!(v.GetType() == typeof(IMarshaler)))
        {
            return (new byte[] { }, new errors.Error($"{ErrorMessages.MarshalObjectFailed} was={v.GetType()} expected={typeof(IMarshaler)}"));
        }

        var m = v as IMarshaler;
        if (m == null)
        {
            return (new byte[] { }, new errors.Error(ErrorMessages.ObjectAsMarshalerFailed));
        }
        return m.MarshalMux();
    }
    public static errors.Error? Unmarshal(byte[] b, object v)
    {
        if (!(v.GetType() == typeof(IUnmarshaler)))
        {
            return new errors.Error($"{ErrorMessages.UnmarshalObjectFailed} was={v.GetType()} expected={typeof(IUnmarshaler)}");
        }
        var u = v as IUnmarshaler;
        if (u == null)
        {
            return new errors.Error($"{ErrorMessages.UnmarshalUnsupportedField} for value={v.GetType()}");
        }

        return u.UnmarshalMux(b);
    }

    public static (IMessage?, errors.Error?) Decode(byte[] packet)
    {
        if (packet.Length == 0)
        {
            return (null, new errors.Error(ErrorMessages.DecodeFailedByteArrayEmpty));
        }

        IMessage? msg;
        switch ((MessageType)packet[0])
        {
            case (MessageType.MessageChannelOpen):
                msg = new OpenMessage();
                break;
            case (MessageType.MessageChannelData):
                msg = new DataMessage();
                break;
            case (MessageType.MessageChannelOpenConfirm):
                msg = new OpenConfirmMessage();
                break;
            case (MessageType.MessageChannelOpenFailure):
                msg = new OpenFailureMessage();
                break;
            case (MessageType.MessageChannelWindowAdjust):
                msg = new WindowAdjustMessage();
                break;
            case (MessageType.MessageChannelEOF):
                msg = new EofMessage();
                break;
            case (MessageType.MessageChannelClose):
                msg = new CloseMessage();
                break;
            default:
                return (null, new errors.Error($"{ErrorMessages.UnexpectedMessageType}: was={packet[0]}"));
        }

        var err = Unmarshal(packet, msg);
        if (err != null)
        {
            return (null, err);
        }

        if (DebugMessages != null)
        {
            var debugBytes = Encoding.UTF8.GetBytes($"<<DEC {msg}");
            DebugMessages.Write(debugBytes);
        }
        return (msg, null);
    }

    public static (byte[], errors.Error?) ReadPacket(io.IReader c)
    {
        var msgNum = new byte[1];
        var (_, err) = c.Read(msgNum);
        if (err != null)
        {
            // TODO @stevemurr: find syscall.ECONNRESET equivalent
            // @progrium: this is the equivalent of golang panic
            // this method writes a json file to disk where app was run containing stack trace, memory contents, etc.
            System.Environment.FailFast("failed to read packet");
        }

        var payloadSizeType = (PayloadSizes)msgNum[0];

        // NOTE: this cast may fail
        var rest = new byte[(int)payloadSizeType];
        (_, err) = c.Read(rest);
        if (err != null)
        {
            return (new byte[0], err);
        }

        // @stevemurr: potential perf investigation in future - not sure of the byte[] array allocation costs for csharp
        // @progrium: System.Buffer.BlockCopy is the most efficient way to concatenate primitive types
        // https://docs.microsoft.com/en-us/dotnet/api/system.buffer.blockcopy?view=net-6.0 
        // TODO @stevemurr: unit test ... the block copies probably have a bug lol
        // packet := append(msgNum, rest...)
        var packet = new byte[(int)payloadSizeType + 1];
        System.Buffer.BlockCopy(msgNum, 0, packet, 0, 1);
        System.Buffer.BlockCopy(rest, 0, packet, 1, rest.Length - 1);

        if ((MessageType)msgNum[0] == MessageType.MessageChannelData)
        {
            var stream = new MemoryStream(msgNum);
            using (var reader = new BinaryReader(stream, Encoding.BigEndianUnicode))
            {
                // move pointer 4 bytes
                // NOTE: @stevemurr look for seek method again
                reader.ReadUInt32();
                var dataSize = reader.ReadUInt32();
                var data = new byte[dataSize];
                (_, err) = c.Read(data);
                if (err != null)
                {
                    return (new byte[0], err);
                }

                System.Buffer.BlockCopy(data, 0, packet, (int)payloadSizeType + 1, data.Length - 1);
            }
        }
        return (packet, null);
    }
}