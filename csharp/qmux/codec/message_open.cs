namespace qmux.codec;

using System.Text;
public struct OpenMessage : Message
{
    public UInt32 SenderId;
    public UInt32 WindowSize;
    public UInt32 MaxPacketSize;
    public string String()
    {
        return $"OpenMessage SenderId:{this.SenderId} WindowSize:{this.WindowSize} MaxPacketSize:{this.MaxPacketSize}";
    }
    public (UInt32, bool) Channel()
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
            writer.Write(this.SenderId);
            // 5-9
            writer.Write(this.WindowSize);
            // 10-14
            writer.Write(this.MaxPacketSize);
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
            this.SenderId = reader.ReadUInt32();
            // 5-9
            this.WindowSize = reader.ReadUInt32();
            // 10-14
            this.MaxPacketSize = reader.ReadUInt32();
        }
    }
}