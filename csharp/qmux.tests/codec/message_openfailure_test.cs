namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestOpenFailureMessage
{
    [Fact]
    public void Test_OpenFailureMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new codec.OpenFailureMessage()
        {
            ChannelId = 10,
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new codec.OpenFailureMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
    }
}