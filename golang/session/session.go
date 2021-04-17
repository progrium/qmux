package session

import (
	"context"
	"fmt"
	"io"
	"sync"

	mux "github.com/progrium/qmux/golang"
	"github.com/progrium/qmux/golang/codec"
)

const (
	minPacketLength = 9
	maxPacketLength = 1 << 31

	// channelMaxPacket contains the maximum number of bytes that will be
	// sent in a single packet. As per RFC 4253, section 6.1, 32k is also
	// the minimum.
	channelMaxPacket = 1 << 15
	// We follow OpenSSH here.
	channelWindowSize = 64 * channelMaxPacket

	// chanSize sets the amount of buffering qmux connections. This is
	// primarily for testing: setting chanSize=0 uncovers deadlocks more
	// quickly.
	chanSize = 16
)

type session struct {
	conn     io.ReadWriteCloser
	chanList chanList

	enc *codec.Encoder
	dec *codec.Decoder

	incomingChannels chan mux.Channel

	errCond *sync.Cond
	err     error
	closeCh chan bool
}

// NewSession returns a session that runs over the given connection.
func New(rwc io.ReadWriteCloser) mux.Session {
	if rwc == nil {
		return nil
	}
	s := &session{
		conn:             rwc,
		enc:              codec.NewEncoder(rwc),
		dec:              codec.NewDecoder(rwc),
		incomingChannels: make(chan mux.Channel, chanSize),
		errCond:          sync.NewCond(new(sync.Mutex)),
		closeCh:          make(chan bool, 1),
	}
	go s.loop()
	return s
}

func (s *session) Close() error {
	s.conn.Close()
	return nil
}

func (s *session) Wait() error {
	s.errCond.L.Lock()
	defer s.errCond.L.Unlock()
	for s.err == nil {
		s.errCond.Wait()
	}
	return s.err
}

func (s *session) Accept() (mux.Channel, error) {
	select {
	case ch := <-s.incomingChannels:
		return ch, nil
	case <-s.closeCh:
		return nil, io.EOF
	}
}

func (s *session) Open(ctx context.Context) (mux.Channel, error) {
	ch := s.newChannel(channelOutbound)
	ch.maxIncomingPayload = channelMaxPacket

	if err := s.enc.Encode(codec.OpenMessage{
		WindowSize:    ch.myWindow,
		MaxPacketSize: ch.maxIncomingPayload,
		SenderID:      ch.localId,
	}); err != nil {
		return nil, err
	}

	var m codec.Message

	select {
	case <-ctx.Done():
		return nil, ctx.Err()
	case m = <-ch.msg:
		if m == nil {
			return nil, fmt.Errorf("qmux: channel closed early during open")
		}
	}

	switch msg := m.(type) {
	case *codec.OpenConfirmMessage:
		return ch, nil
	case *codec.OpenFailureMessage:
		return nil, fmt.Errorf("qmux: channel open failed on remote side")
	default:
		return nil, fmt.Errorf("qmux: unexpected packet in response to channel open: %v", msg)
	}
}

func (s *session) newChannel(direction channelDirection) *channel {
	ch := &channel{
		remoteWin: window{Cond: sync.NewCond(new(sync.Mutex))},
		myWindow:  channelWindowSize,
		pending:   newBuffer(),
		direction: direction,
		msg:       make(chan codec.Message, chanSize),
		session:   s,
		packetBuf: make([]byte, 0),
	}
	ch.localId = s.chanList.add(ch)
	return ch
}

// loop runs the connection machine. It will process packets until an
// error is encountered. To synchronize on loop exit, use session.Wait.
func (s *session) loop() {
	var err error
	for err == nil {
		err = s.onePacket()
	}

	for _, ch := range s.chanList.dropAll() {
		ch.close()
	}

	s.conn.Close()
	s.closeCh <- true

	s.errCond.L.Lock()
	s.err = err
	s.errCond.Broadcast()
	s.errCond.L.Unlock()
}

// onePacket reads and processes one packet.
func (s *session) onePacket() error {
	var err error
	var msg codec.Message

	msg, err = s.dec.Decode()
	if err != nil {
		return err
	}

	id, isChan := msg.Channel()
	if !isChan {
		return s.handleOpen(msg.(*codec.OpenMessage))
	}

	ch := s.chanList.getChan(id)
	if ch == nil {
		return fmt.Errorf("qmux: invalid channel %d", id)
	}

	return ch.handle(msg)
}

// handleChannelOpen schedules a channel to be Accept()ed.
func (s *session) handleOpen(msg *codec.OpenMessage) error {
	if msg.MaxPacketSize < minPacketLength || msg.MaxPacketSize > maxPacketLength {
		return s.enc.Encode(codec.OpenFailureMessage{
			ChannelID: msg.SenderID,
		})
	}

	c := s.newChannel(channelInbound)
	c.remoteId = msg.SenderID
	c.maxRemotePayload = msg.MaxPacketSize
	c.remoteWin.add(msg.WindowSize)
	c.maxIncomingPayload = channelMaxPacket
	s.incomingChannels <- c

	return s.enc.Encode(codec.OpenConfirmMessage{
		ChannelID:     c.remoteId,
		SenderID:      c.localId,
		WindowSize:    c.myWindow,
		MaxPacketSize: c.maxIncomingPayload,
	})
}
