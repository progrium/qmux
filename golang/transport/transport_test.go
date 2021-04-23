package transport

import (
	"bytes"
	"context"
	"io"
	"io/ioutil"
	"testing"

	mux "github.com/progrium/qmux/golang"
)

func fatal(err error, t *testing.T) {
	t.Helper()
	if err != nil {
		t.Fatal(err)
	}
}

func testExchange(t *testing.T, sess mux.Session) {
	var err error
	var ch mux.Channel
	t.Run("session accept", func(t *testing.T) {
		ch, err = sess.Accept()
		fatal(err, t)
	})

	t.Run("channel write", func(t *testing.T) {
		_, err = ch.Write([]byte("Hello world"))
		fatal(err, t)
		err = ch.Close()
		fatal(err, t)
	})

	t.Run("session open", func(t *testing.T) {
		ch, err = sess.Open(context.Background())
		fatal(err, t)
	})

	var b []byte
	t.Run("channel read", func(t *testing.T) {
		b, err = ioutil.ReadAll(ch)
		fatal(err, t)
		ch.Close() // should already be closed by other end
	})

	if !bytes.Equal(b, []byte("Hello world")) {
		t.Fatalf("unexpected bytes: %s", b)
	}

	t.Run("session close", func(t *testing.T) {
		err = sess.Close()
		fatal(err, t)
	})
}

func startListener(t *testing.T, l Listener) {
	t.Helper()

	t.Cleanup(func() {
		fatal(l.Close(), t)
	})

	go func() {
		sess, err := l.Accept()
		fatal(err, t)
		t.Cleanup(func() {
			// Synchronizes cleanup, waiting for the client to disconnect before
			// closing the stream. This prevents errors in the Pipe-based test with
			// closing one end of the pipe before the other has read the data.
			// Registering as a test cleanup function also avoids a race condition
			// with the test existing before closing the session.
			if err := mux.Wait(sess); err != io.EOF {
				t.Errorf("Wait returned unexpected error: %v", err)
			}
			err = sess.Close()
			fatal(err, t)
		})

		ch, err := sess.Open(context.Background())
		fatal(err, t)
		b, err := ioutil.ReadAll(ch)
		fatal(err, t)
		ch.Close() // should already be closed by other end

		ch, err = sess.Accept()
		_, err = ch.Write(b)
		fatal(err, t)
		err = ch.CloseWrite()
		fatal(err, t)
	}()
}

func TestTCP(t *testing.T) {
	l, err := ListenTCP("127.0.0.1:0")
	fatal(err, t)
	startListener(t, l)

	sess, err := DialTCP(l.Addr().String())
	fatal(err, t)
	testExchange(t, sess)
}

func TestIO(t *testing.T) {
	pr1, pw1 := io.Pipe()
	pr2, pw2 := io.Pipe()

	l, err := ListenIO(pw1, pr2)
	fatal(err, t)
	startListener(t, l)

	sess, err := DialIO(pw2, pr1)
	fatal(err, t)
	testExchange(t, sess)
}

func TestWS(t *testing.T) {
	l, err := ListenWS("127.0.0.1:0")
	fatal(err, t)
	startListener(t, l)

	sess, err := DialWS(l.Addr().String())
	fatal(err, t)
	testExchange(t, sess)
}
