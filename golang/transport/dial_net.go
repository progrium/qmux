package transport

import (
	"net"

	mux "github.com/progrium/qmux/golang"
	"github.com/progrium/qmux/golang/session"
)

func dialNet(proto, addr string) (mux.Session, error) {
	conn, err := net.Dial(proto, addr)
	if err != nil {
		return nil, err
	}
	return session.New(conn), nil
}

func DialTCP(addr string) (mux.Session, error) {
	return dialNet("tcp", addr)
}

func DialUnix(addr string) (mux.Session, error) {
	return dialNet("unix", addr)
}
