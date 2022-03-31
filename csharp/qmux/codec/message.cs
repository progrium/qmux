namespace qmux.codec;

public interface IMessage
{
    // Starting in C# 9.0, you can use the nint and nuint keywords to define native-sized integers. 
    // These are 32-bit integers when running in a 32-bit process, or 64-bit integers when running in a 64-bit process. 
    // They can be used for interop scenarios, low-level libraries, and to optimize performance in scenarios where integer math is used extensively.
    public (nuint, bool) Channel();
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