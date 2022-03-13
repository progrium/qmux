namespace qmux.codec;

using System;
using System.IO;
using System.Text;

public struct DataMessage
{
    public UInt32 ChannelId;
    public UInt32 Length;
    public byte[] Data;

    public string String()
    {
        return $"DataMessage ChannelId:{this.ChannelId} Length:{this.Length} Data:...";
    }
    public (UInt32, bool) Channel()
    {
        return (this.ChannelId, true);
    }

    public byte[] MarshalMux()
    {
        // 8 + 1 + this.Data.Length
        var bufferSize = (int)PayloadSizes.MessageChannelData + 1 + this.Length;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelData);
            // 1-5
            writer.Write(this.ChannelId);
            // 5-9
            writer.Write(this.Length);
            // 10..
            writer.Write(this.Data);
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
            this.ChannelId = reader.ReadUInt32();
            // 5-9
            this.Length = reader.ReadUInt32();
            // 10..
            this.Data = reader.ReadBytes((int)this.Length);
        }
    }
}