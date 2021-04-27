package mux

import (
	"context"
	"io"
)

// Session is a bi-directional channel muxing session on a given transport.
type Session interface {
	// Close closes the underlying transport.
	// Any blocked Accept operations will be unblocked and return errors.
	Close() error

	// Open establishes a new channel with the other end.
	Open(ctx context.Context) (Channel, error)

	// Accept waits for and returns the next incoming channel.
	Accept() (Channel, error)
}

// Channel is an ordered, reliable, flow-controlled, duplex stream
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

// Transport is an interface describing what is needed for a session
type Transport interface {
	io.Reader
	io.Writer
	io.Closer
}
