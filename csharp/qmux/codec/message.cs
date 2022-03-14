namespace qmux.codec;

public interface IMessage
{
    public (UInt32, bool) Channel();
    public string String();
}

public enum MessageType : byte
{
    MessageChannelOpen = 100,
    MessageChannelOpenConfirm = 101,
    MessageChannelOpenFailure = 102,
    MessageChannelWindowAdjust = 103,
    MessageChannelData = 104,
    MessageChannelEOF = 105,
    MessageChannelClose = 106
}

public enum PayloadSizes : byte
{
    MessageChannelOpen = 12,
    MessageChannelOpenConfirm = 16,
    MessageChannelOpenFailure = 4,
    MessageChannelWindowAdjust = 8,
    MessageChannelData = 8,
    MessageChannelEOF = 4,
    MessageChannelClose = 4
}