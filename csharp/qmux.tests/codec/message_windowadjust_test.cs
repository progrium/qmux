namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestWindowAdjustMessage
{
    [Fact]
    public void Test_WindowAdjustMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new codec.WindowAdjustMessage()
        {
            ChannelId = 10,
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new codec.WindowAdjustMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
        Assert.Equal(msg1.AdditionalBytes, msg2.AdditionalBytes);
    }
}