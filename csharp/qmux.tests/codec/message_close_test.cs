namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestCloseMessage
{
    [Fact]
    public void Test_CloseMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new CloseMessage()
        {
            ChannelId = 10,
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new CloseMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
    }
}