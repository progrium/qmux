namespace qmux.codec;

using System;
using System.IO;
using System.Text;

public struct WindowAdjustMessage
{
    public UInt32 ChannelId;
    public UInt32 AdditionalBytes;
    public string String()
    {
        return $"OpenConfirmMessage ChannelId:{this.ChannelId} AdditionalBytes:{this.AdditionalBytes}";
    }
    public (UInt32, bool) Channel()
    {
        return (this.ChannelId, true);
    }

    public byte[] MarshalMux()
    {
        // 8 + 1
        var bufferSize = (int)PayloadSizes.MessageChannelWindowAdjust + 1;
        var stream = new MemoryStream(new byte[bufferSize]);
        using (var writer = new BinaryWriter(stream, Encoding.BigEndianUnicode, false))
        {
            // 0
            writer.Write((byte)MessageType.MessageChannelWindowAdjust);
            // 1-5
            writer.Write(this.ChannelId);
            writer.Write(this.AdditionalBytes);
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
            this.AdditionalBytes = reader.ReadUInt32();
        }
    }
}