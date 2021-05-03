package transport

import (
	"io"
	"os"

	"github.com/progrium/qmux/golang/mux"
	"github.com/progrium/qmux/golang/session"
)

func DialIO(out io.WriteCloser, in io.ReadCloser) (mux.Session, error) {
	return session.New(&ioduplex{out, in}), nil
}

func DialStdio() (mux.Session, error) {
	return DialIO(os.Stdout, os.Stdin)
}
