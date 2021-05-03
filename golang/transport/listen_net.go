package transport

import (
	"io"
	"net"

	"github.com/progrium/qmux/golang/mux"
	"github.com/progrium/qmux/golang/session"
)

type NetListener struct {
	net.Listener
	accepted chan mux.Session
	closer   chan bool
	errs     chan error
}

func (l *NetListener) Accept() (mux.Session, error) {
	select {
	case <-l.closer:
		return nil, io.EOF
	case err := <-l.errs:
		return nil, err
	case sess := <-l.accepted:
		return sess, nil
	}
}

// func (l *NetListener) Addr() net.Addr {
// 	return l.Addr()
// }

func (l *NetListener) Close() error {
	if l.closer != nil {
		l.closer <- true
	}
	return l.Listener.Close()
}

func listenNet(proto, addr string) (*NetListener, error) {
	l, err := net.Listen(proto, addr)
	if err != nil {
		return nil, err
	}
	closer := make(chan bool, 1)
	errs := make(chan error, 1)
	accepted := make(chan mux.Session)
	go func(l net.Listener) {
		for {
			conn, err := l.Accept()
			if err != nil {
				errs <- err
				return
			}
			accepted <- session.New(conn)
		}
	}(l)
	return &NetListener{
		Listener: l,
		errs:     errs,
		accepted: accepted,
		closer:   closer,
	}, nil
}

func ListenTCP(addr string) (*NetListener, error) {
	return listenNet("tcp", addr)
}

func ListenUnix(addr string) (*NetListener, error) {
	return listenNet("unix", addr)
}
