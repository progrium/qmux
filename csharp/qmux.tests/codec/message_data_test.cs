namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestDataMessage
{
    [Fact]
    public void Test_DataMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new DataMessage()
        {
            ChannelId = 10,
            Length = 5,
            Data = new byte[5] { 0x20, 0x20, 0x20, 0x20, 0x20 },
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new DataMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
        Assert.Equal(msg1.Length, msg2.Length);
        Assert.Equal(msg1.Data, msg2.Data);
    }
}