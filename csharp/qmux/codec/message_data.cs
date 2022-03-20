namespace qmux.codec;

using System.IO;
using System.Text;

public struct DataMessage : IMessage
{
    private nuint channelId;
    private nuint length;
    private byte[] data;

    public string String()
    {
        return $"DataMessage ChannelId:{this.channelId} Length:{this.length} Data:...";
    }
    public (nuint, bool) Channel()
    {
        return (this.channelId, true);
    }

    public byte[] MarshalMux()
    {
        // 8 + 1 + this.Data.Length
        var bufferSize = (int)PayloadSizes.MessageChannelData + 1 + this.length;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelData);
            // 1-5
            writer.Write(this.channelId);
            // 5-9
            writer.Write(this.length);
            // 10..
            writer.Write(this.data);
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
            // 5-9
            this.length = reader.ReadUInt32();
            // 10..
            this.data = reader.ReadBytes((int)this.length);
        }
    }
}