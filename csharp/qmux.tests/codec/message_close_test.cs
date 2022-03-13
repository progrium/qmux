namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestCloseMessage
{
    [Fact]
    public void Test_CloseMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var dataMessage = new CloseMessage()
        {
            ChannelId = 10,
        };

        var marshaled = dataMessage.MarshalMux();
        var dataMessage2 = new CloseMessage();
        dataMessage2.UnmarshalMux(marshaled);
        Assert.Equal(dataMessage.ChannelId, dataMessage2.ChannelId);
    }
}