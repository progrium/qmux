namespace qmux.session;

using codec = qmux.codec;

// Channel is an implementation of the Channel interface that works
// with the Session class.
public struct Channel
{
    public UInt32 LocalId;
    public UInt32 RemoteId;
    public UInt32 MaxIncomingPayload;
    public UInt32 maxRemotePayload;
    public Session Session;
    public ChannelDirection Direction;
    // TODO @stevemurr: this is a chan
    // TODO @stevemurr: establish equivalent chan pattern 
    public codec.IMessage message;
    public bool sentOEF;
    public IWindow remoteWin;
    // TODO @stevemurr: what type is *buffer
    // pending* buffer

    // WindowMutex protects MyWindow
    // MyWindow is the flow-control window
    private static Mutex WindowMutex = new Mutex();
    public UInt32 MyWindow;

    // writeMu serializes calls to session.conn.Write() and
    // protects sentClose and packetPool. This mutex must be
    // different from windowMu, as writePacket can block if there
    // is a key exchange pending.
    private static Mutex WriteMutex = new Mutex();
    public bool SentClose;
    public byte[] PacketBuffer;
}

// ID returns the unique identifier of this channel
// within the session
// func(ch * Channel) ID() uint32
// {
//     return ch.localId
// }

// // CloseWrite signals the end of sending data.
// // The other side may still send data
// func(ch * Channel) CloseWrite() error
// {
//     ch.sentEOF = true

//     return ch.send(codec.EOFMessage{
//     ChannelID: ch.remoteId})
// }

// // Close signals end of channel use. No data may be sent after this
// // call.
// func(ch * Channel) Close() error
// {
//     return ch.send(codec.CloseMessage{
//     ChannelID: ch.remoteId})
// }

// // Write writes len(data) bytes to the channel.
// func(ch * Channel) Write(data[]byte)(n int, err error) {
//     if ch.sentEOF {
//         return 0, io.EOF

//     }

//     for len(data) > 0 {
//     space:= min(ch.maxRemotePayload, len(data))

//         if space, err = ch.remoteWin.reserve(space); err != nil {
//             return n, err

//         }

//     toSend:= data[:space]


//         if err = ch.session.enc.Encode(codec.DataMessage{
//         ChannelID: ch.remoteId,
// 			Length: uint32(len(toSend)),
// 			Data: toSend,
// 		}); err != nil {
//         return n, err

//         }

//     n += len(toSend)

//         data = data[len(toSend):]

//     }

// return n, err
// }

// // Read reads up to len(data) bytes from the channel.
// func(c * Channel) Read(data[]byte)(n int, err error) {
//     n, err = c.pending.Read(data)


//     if n > 0 {
//         err = c.adjustWindow(uint32(n))
//         // sendWindowAdjust can return io.EOF if the remote
//         // peer has closed the connection, however we want to
//         // defer forwarding io.EOF to the caller of Read until
//         // the buffer has been drained.
//         if n > 0 && err == io.EOF {
//             err = nil

//         }
//     }
//     return n, err
// }

// // writePacket sends a packet. If the packet is a channel close, it updates
// // sentClose. This method takes the lock c.writeMu.
// func(ch * Channel) send(msg interface{ }) error
// {
//     ch.writeMu.Lock()

//     defer ch.writeMu.Unlock()


//     if ch.sentClose {
//         return io.EOF

//     }

//     if _, ok:= msg.(codec.CloseMessage); ok {
//         ch.sentClose = true

//     }

//     return ch.session.enc.Encode(msg)
// }

// func(c * Channel) adjustWindow(n uint32) error
// {
//     c.windowMu.Lock()
//     // Since myWindow is managed on our side, and can never exceed
//     // the initial window setting, we don't worry about overflow.
//     c.myWindow += uint32(n)

//     c.windowMu.Unlock()

//     return c.send(codec.WindowAdjustMessage{
//     ChannelID: c.remoteId,
// 		AdditionalBytes: uint32(n),
// 	})
// }

// func(c * Channel) close() {
//     c.pending.eof()

//     close(c.msg)

//     c.writeMu.Lock()
//     // This is not necessary for a normal channel teardown, but if
//     // there was another error, it is.
//     c.sentClose = true

//     c.writeMu.Unlock()
//     // Unblock writers.
//     c.remoteWin.close()
// }

// // responseMessageReceived is called when a success or failure message is
// // received on a channel to check that such a message is reasonable for the
// // given channel.
// func(ch * Channel) responseMessageReceived() error
// {
//     if ch.direction == channelInbound {
//         return errors.New("qmux: channel response message received on inbound channel")

//     }
//     return nil
// }

// func(ch * Channel) handle(msg codec.Message) error
// {
//     switch m := msg.(type) {
// 	case *codec.DataMessage:
// 		return ch.handleData(m)

// 	case *codec.CloseMessage:
// 		ch.send(codec.CloseMessage{
//         ChannelID: ch.remoteId,
// 		})
// 		ch.session.chans.remove(ch.localId)

//         ch.close()

//         return nil

// 	case *codec.EOFMessage:
// 		ch.pending.eof()

//         return nil

// 	case *codec.WindowAdjustMessage:
// 		if !ch.remoteWin.add(m.AdditionalBytes) {
//             return fmt.Errorf("qmux: invalid window update for %d bytes", m.AdditionalBytes)

//         }
//         return nil

// 	case *codec.OpenConfirmMessage:
// 		if err := ch.responseMessageReceived(); err != nil {
//             return err

//         }
//         if m.MaxPacketSize < minPacketLength || m.MaxPacketSize > maxPacketLength {
//             return fmt.Errorf("qmux: invalid MaxPacketSize %d from peer", m.MaxPacketSize)

//         }
//         ch.remoteId = m.SenderID

//         ch.maxRemotePayload = m.MaxPacketSize

//         ch.remoteWin.add(m.WindowSize)

//         ch.msg < -m

//         return nil

// 	case *codec.OpenFailureMessage:
// 		if err := ch.responseMessageReceived(); err != nil {
//             return err

//         }
//         ch.session.chans.remove(m.ChannelID)

//         ch.msg < -m

//         return nil


//     default:
// 		return fmt.Errorf("qmux: invalid channel message %v", msg)

//     }
// }

// func(ch * Channel) handleData(msg * codec.DataMessage) error
// {
//     if msg.Length > ch.maxIncomingPayload {
//         // TODO(hanwen): should send Disconnect?
//         return errors.New("qmux: incoming packet exceeds maximum payload size")

//     }

//     if msg.Length != uint32(len(msg.Data)) {
//         return errors.New("qmux: wrong packet length")

//     }

//     ch.windowMu.Lock()

//     if ch.myWindow < msg.Length {
//         ch.windowMu.Unlock()
//         // TODO(hanwen): should send Disconnect with reason?
//         return errors.New("qmux: remote side wrote too much")

//     }
//     ch.myWindow -= msg.Length

//     ch.windowMu.Unlock()


//     ch.pending.write(msg.Data)

//     return nil
// }
