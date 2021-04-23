package transport

import mux "github.com/progrium/qmux/golang"

type Listener interface {
	Close() error
	Accept() (mux.Session, error)
}
