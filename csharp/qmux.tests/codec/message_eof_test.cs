namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestEofMessage
{
    [Fact]
    public void Test_EofMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var dataMessage = new EofMessage()
        {
            ChannelId = 10,
        };

        var marshaled = dataMessage.MarshalMux();
        var dataMessage2 = new EofMessage();
        dataMessage2.UnmarshalMux(marshaled);
        Assert.Equal(dataMessage.ChannelId, dataMessage2.ChannelId);
    }
}