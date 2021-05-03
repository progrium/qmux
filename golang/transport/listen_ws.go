package transport

import (
	"net"
	"net/http"

	"github.com/progrium/qmux/golang/mux"
	"github.com/progrium/qmux/golang/session"
	"golang.org/x/net/websocket"
)

func HandleWS(l *NetListener, ws *websocket.Conn) {
	ws.PayloadType = websocket.BinaryFrame
	sess := session.New(ws)
	defer sess.Close()
	l.accepted <- sess
	l.errs <- mux.Wait(sess)
}

func ListenWS(addr string) (*NetListener, error) {
	l, err := net.Listen("tcp", addr)
	if err != nil {
		return nil, err
	}
	nl := &NetListener{
		Listener: l,
		accepted: make(chan mux.Session),
		errs:     make(chan error, 2),
		closer:   make(chan bool, 1),
	}
	s := &http.Server{
		Addr: addr,
		Handler: websocket.Handler(func(ws *websocket.Conn) {
			HandleWS(nl, ws)
		}),
	}
	go func() {
		nl.errs <- s.Serve(l)
	}()
	return nl, nil
}
