namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestOpenMessage
{
    [Fact]
    public void Test_OpenMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var msg1 = new codec.OpenMessage()
        {
            SenderId = 10,
            WindowSize = 10,
            MaxPacketSize = 10
        };

        var marshaled = msg1.MarshalMux();
        var msg2 = new codec.OpenMessage();
        msg2.UnmarshalMux(marshaled);
        Assert.Equal(msg1.SenderId, msg2.SenderId);
        Assert.Equal(msg1.WindowSize, msg2.WindowSize);
        Assert.Equal(msg1.MaxPacketSize, msg2.MaxPacketSize);
    }
}