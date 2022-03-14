namespace qmux.codec;

using System;
using System.IO;
using System.Text;

public struct OpenConfirmMessage : IMessage
{
    public UInt32 ChannelId;
    public UInt32 SenderId;
    public UInt32 WindowSize;
    public UInt32 MaxPacketSize;

    public string String()
    {
        return $"OpenConfirmMessage ChannelId:{this.ChannelId} SenderId:{this.SenderId} WindowSize:{this.WindowSize} MaxPacketSize:{this.MaxPacketSize}";
    }
    public (UInt32, bool) Channel()
    {
        return (this.ChannelId, true);
    }

    public byte[] MarshalMux()
    {
        // 16 + 1
        var bufferSize = (int)PayloadSizes.MessageChannelOpenConfirm + 1;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelOpenConfirm);
            // 1-5
            writer.Write(this.ChannelId);
            // 5-9
            writer.Write(this.SenderId);
            writer.Write(this.WindowSize);
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
            this.ChannelId = reader.ReadUInt32();
            this.SenderId = reader.ReadUInt32();
            this.WindowSize = reader.ReadUInt32();
            this.MaxPacketSize = reader.ReadUInt32();
        }
    }
}