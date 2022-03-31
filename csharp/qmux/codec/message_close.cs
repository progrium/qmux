namespace qmux.codec;

using System.IO;
using System.Text;

public class CloseMessage : IMessage
{
    private nuint channelId;
    public CloseMessage() { }
    public CloseMessage(nuint channelId)
    {
        this.channelId = channelId;
    }
    public string String()
    {
        return $"CloseMessage ChannelId: {this.channelId}";
    }

    public (nuint, bool) Channel()
    {
        return (this.channelId, true);
    }

    public byte[] MarshalMux()
    {
        // 4 + 1
        var bufferSize = (int)PayloadSizes.MessageChannelClose + 1;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelClose);
            // 1-5
            writer.Write(this.channelId);
        }
        return stream.ToArray();
    }

    public void UnmarshalMux(byte[] b)
    {
        var stream = new MemoryStream(b);
        using (var reader = new BinaryReader(stream, Encoding.BigEndianUnicode))
        {
            // 0
            reader.ReadByte();
            // 1-5
            this.channelId = reader.ReadUInt32();
        }
    }
}