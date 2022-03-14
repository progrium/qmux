namespace qmux.session;

using codec = qmux.codec;
using errors = gostdlib.errors;
using mux = qmux.mux;

public enum ChannelDirection : byte
{
    Inbound = 0,
    Outbound
}

internal static class ErrorMesasges
{
    public static string RemoteSideWroteTooMuch = "qmux: remote side wrote too much";
}

public static class G
{
    public static byte MinPacketLength = 9;
    public static int MaxPacketLength = 1 << 31;

    // channelMaxPacket contains the maximum number of bytes that will be
    // sent in a single packet. As per RFC 4253, section 6.1, 32k is also
    // the minimum.
    public static int ChannelMaxPacket = 1 << 15;

    // We follow OpenSSH here.
    public static int ChannelWindowSize = 64 * ChannelMaxPacket;

    // chanSize sets the amount of buffering qmux connections. This is
    // primarily for testing: setting chanSize=0 uncovers deadlocks more
    // quickly.
    public static int ChanSize = 16;
}

public struct Session
{
    public mux.Transport t;

    // chans chanList

    public codec.Encoder enc;
    public codec.Decoder dec;

    // TODO @stevemurr: this is a chan
    public mux.Channel inbox;

    public errors.Error? error;

    // errCond sync.Cond
    // closeCh chan bool
}

// Session is a bi-directional channel muxing session on a given transport.
// type Session struct {
// 	t     mux.Transport
// 	chans chanList

// 	enc *codec.Encoder
// 	dec *codec.Decoder

// 	inbox chan mux.Channel

// 	errCond *sync.Cond
// 	err     error
// 	closeCh chan bool
// }