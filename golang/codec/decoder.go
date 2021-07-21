package codec

import (
	"encoding/binary"
	"errors"
	"fmt"
	"io"
	"os"
	"sync"
	"syscall"
)

type Decoder struct {
	r io.Reader
	sync.Mutex
}

func NewDecoder(r io.Reader) *Decoder {
	return &Decoder{r: r}
}

func (dec *Decoder) Decode() (Message, error) {
	dec.Lock()
	defer dec.Unlock()

	packet, err := readPacket(dec.r)
	if err != nil {
		return nil, err
	}

	if DebugBytes != nil {
		fmt.Fprintln(DebugBytes, ">>DEC", packet)
	}

	return decode(packet)
}

func readPacket(c io.Reader) ([]byte, error) {
	msgNum := make([]byte, 1)
	_, err := c.Read(msgNum)
	if err != nil {
		var syscallErr *os.SyscallError
		if errors.As(err, &syscallErr) && syscallErr.Err == syscall.ECONNRESET {
			return nil, io.EOF
		}
		return nil, err
	}

	rest := make([]byte, payloadSizes[msgNum[0]])
	_, err = c.Read(rest)
	if err != nil {
		return nil, err
	}

	packet := append(msgNum, rest...)

	if msgNum[0] == msgChannelData {
		dataSize := binary.BigEndian.Uint32(rest[4:8])
		data := make([]byte, dataSize)
		_, err := c.Read(data)
		if err != nil {
			return nil, err
		}

		packet = append(packet, data...)
	}

	return packet, nil
}

func decode(packet []byte) (Message, error) {
	var msg Message
	switch packet[0] {
	case msgChannelOpen:
		msg = new(OpenMessage)
	case msgChannelData:
		msg = new(DataMessage)
	case msgChannelOpenConfirm:
		msg = new(OpenConfirmMessage)
	case msgChannelOpenFailure:
		msg = new(OpenFailureMessage)
	case msgChannelWindowAdjust:
		msg = new(WindowAdjustMessage)
	case msgChannelEOF:
		msg = new(EOFMessage)
	case msgChannelClose:
		msg = new(CloseMessage)
	default:
		return nil, fmt.Errorf("qmux: unexpected message type %d", packet[0])
	}
	if err := Unmarshal(packet, msg); err != nil {
		return nil, err
	}
	if DebugMessages != nil {
		fmt.Fprintln(DebugMessages, ">>DEC", msg)
	}
	return msg, nil
}

type Unmarshaler interface {
	UnmarshalMux([]byte) error
}

func Unmarshal(b []byte, v interface{}) error {
	u, ok := v.(Unmarshaler)
	if !ok {
		return fmt.Errorf("qmux: unmarshal not supported for value %#v", v)
	}
	return u.UnmarshalMux(b)
}
