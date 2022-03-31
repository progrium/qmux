namespace qmux.codec;

using System.IO;
using System.Text;

public struct OpenMessage : IMessage
{
    private nuint senderId;
    private nuint windowSize;
    private nuint maxPacketSize;
    public string String()
    {
        return $"OpenMessage SenderId:{this.senderId} WindowSize:{this.windowSize} MaxPacketSize:{this.maxPacketSize}";
    }
    public (nuint, bool) Channel()
    {
        return (0, false);
    }
    public byte[] MarshalMux()
    {
        // 12 + 1
        var bufferSize = (int)PayloadSizes.MessageChannelOpen + 1;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelOpen);
            // 1-5
            writer.Write(this.senderId);
            // 5-9
            writer.Write(this.windowSize);
            // 10-14
            writer.Write(this.maxPacketSize);
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
            this.senderId = reader.ReadUInt32();
            // 5-9
            this.windowSize = reader.ReadUInt32();
            // 10-14
            this.maxPacketSize = reader.ReadUInt32();
        }
    }
}