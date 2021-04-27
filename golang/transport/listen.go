package transport

import mux "github.com/progrium/qmux/golang"

type Listener interface {
	// Close closes the listener.
	// Any blocked Accept operations will be unblocked and return errors.
	Close() error

	// Accept waits for and returns the next incoming session.
	Accept() (mux.Session, error)
}
