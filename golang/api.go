package mux

import (
	"context"
	"net"
)

type Session interface {
	Context() context.Context
	Close() error
	Open() (Channel, error)
	Accept() (Channel, error)
	LocalAddr() net.Addr
	RemoteAddr() net.Addr
	Wait() error
}

// A Channel is an ordered, reliable, flow-controlled, duplex stream
// that is multiplexed over a qmux connection.
type Channel interface {
	Context() context.Context

	// Read reads up to len(data) bytes from the channel.
	Read(data []byte) (int, error)

	// Write writes len(data) bytes to the channel.
	Write(data []byte) (int, error)

	// Close signals end of channel use. No data may be sent after this
	// call.
	Close() error

	// CloseWrite signals the end of sending in-band
	// data. The other side may still send data
	CloseWrite() error

	ID() uint32
}

type Listener interface {
	Close() error
	Addr() net.Addr
	Accept() (Session, error)
}
