namespace qmux.tests.codec;

using Xunit;

using qmux.codec;

public class TestDataMessage
{
    [Fact]
    public void Test_DataMessage_MarshalUnmarshalMuxRoundtrip()
    {
        var dataMessage = new DataMessage()
        {
            ChannelId = 10,
            Length = 5,
            Data = new byte[5] { 0x20, 0x20, 0x20, 0x20, 0x20 },
        };

        var marshaled = dataMessage.MarshalMux();
        var dataMessage2 = new DataMessage();
        dataMessage2.UnmarshalMux(marshaled);
        Assert.Equal(dataMessage.ChannelId, dataMessage2.ChannelId);
        Assert.Equal(dataMessage.Length, dataMessage2.Length);
        Assert.Equal(dataMessage.Data, dataMessage2.Data);
    }
}