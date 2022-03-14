namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestOpenConfirmMessage
{
    [Fact]
    public void Test_OpenConfirmMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new codec.OpenConfirmMessage()
        {
            ChannelId = 10,
            SenderId = 10,
            WindowSize = 10,
            MaxPacketSize = 10
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new codec.OpenConfirmMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.ChannelId, msg2.ChannelId);
        Assert.Equal(msg1.SenderId, msg2.SenderId);
        Assert.Equal(msg1.WindowSize, msg2.WindowSize);
        Assert.Equal(msg1.MaxPacketSize, msg2.MaxPacketSize);
    }
}