package transport

import (
	"io"
	"os"

	"github.com/progrium/qmux/golang/mux"
	"github.com/progrium/qmux/golang/session"
)

type IOListener struct {
	io.ReadWriteCloser
}

func (l *IOListener) Accept() (mux.Session, error) {
	return session.New(l.ReadWriteCloser), nil
}

type ioduplex struct {
	io.WriteCloser
	io.ReadCloser
}

func (d *ioduplex) Close() error {
	if err := d.WriteCloser.Close(); err != nil {
		return err
	}
	if err := d.ReadCloser.Close(); err != nil {
		return err
	}
	return nil
}

func ListenIO(out io.WriteCloser, in io.ReadCloser) (*IOListener, error) {
	return &IOListener{
		&ioduplex{out, in},
	}, nil
}

func ListenStdio() (*IOListener, error) {
	return ListenIO(os.Stdout, os.Stdin)
}
