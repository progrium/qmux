namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestEofMessage
{
    [Fact]
    public void Test_EofMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new codec.EofMessage()
        {
            ChannelId = 10,
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new codec.EofMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
    }
}