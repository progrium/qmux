package mux

import "context"

type Session interface {
	Close() error
	Open(ctx context.Context) (Channel, error)
	Accept() (Channel, error)
}

// A Channel is an ordered, reliable, flow-controlled, duplex stream
// that is multiplexed over a qmux session.
type Channel interface {
	// Read reads up to len(data) bytes from the channel.
	Read(data []byte) (int, error)

	// Write writes len(data) bytes to the channel.
	Write(data []byte) (int, error)

	// Close signals end of channel use. No data may be sent after this
	// call.
	Close() error

	// CloseWrite signals the end of sending data.
	// The other side may still send data
	CloseWrite() error

	// ID returns the unique identifier of this channel
	// within the session
	ID() uint32
}
