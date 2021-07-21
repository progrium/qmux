package codec

import (
	"fmt"
	"io"
	"sync"
)

type Encoder struct {
	w io.Writer
	sync.Mutex
}

func NewEncoder(w io.Writer) *Encoder {
	return &Encoder{w: w}
}

func (enc *Encoder) Encode(msg interface{}) error {
	enc.Lock()
	defer enc.Unlock()

	if DebugMessages != nil {
		fmt.Fprintln(DebugMessages, "<<ENC", msg)
	}

	b, err := Marshal(msg)
	if err != nil {
		return err
	}

	if DebugBytes != nil {
		fmt.Fprintln(DebugBytes, "<<ENC", b)
	}

	_, err = enc.w.Write(b)
	return err
}

type Marshaler interface {
	MarshalMux() ([]byte, error)
}

func Marshal(v interface{}) ([]byte, error) {
	m, ok := v.(Marshaler)
	if !ok {
		return []byte{}, fmt.Errorf("qmux: unable to marshal type")
	}
	return m.MarshalMux()
}
