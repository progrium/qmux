package transport

import (
	"fmt"

	mux "github.com/progrium/qmux/golang"
	"github.com/progrium/qmux/golang/session"
	"golang.org/x/net/websocket"
)

func DialWS(addr string) (mux.Session, error) {
	ws, err := websocket.Dial(fmt.Sprintf("ws://%s/", addr), "", fmt.Sprintf("http://%s/", addr))
	if err != nil {
		return nil, err
	}
	ws.PayloadType = websocket.BinaryFrame
	return session.New(ws), nil
}
