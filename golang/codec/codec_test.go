package codec

import (
	"bytes"
	"testing"
)

func TestMarshalUnmarshal(t *testing.T) {
	tests := []struct {
		in  Message
		out Unmarshaler
	}{
		{
			in: CloseMessage{
				ChannelID: 10,
			},
			out: &CloseMessage{},
		},
		{
			in: DataMessage{
				ChannelID: 10,
				Length:    5,
				Data:      []byte("Hello"),
			},
			out: &DataMessage{},
		},
		{
			in: EOFMessage{
				ChannelID: 10,
			},
			out: &EOFMessage{},
		},
		{
			in: OpenMessage{
				SenderID:      10,
				WindowSize:    1024,
				MaxPacketSize: 1 << 31,
			},
			out: &OpenMessage{},
		},
		{
			in: OpenConfirmMessage{
				ChannelID:     20,
				SenderID:      10,
				WindowSize:    1024,
				MaxPacketSize: 1 << 31,
			},
			out: &OpenConfirmMessage{},
		},
		{
			in: OpenFailureMessage{
				ChannelID: 20,
			},
			out: &OpenFailureMessage{},
		},
		{
			in: WindowAdjustMessage{
				ChannelID:       20,
				AdditionalBytes: 1024,
			},
			out: &WindowAdjustMessage{},
		},
	}
	for _, test := range tests {
		b, err := Marshal(test.in)
		if err != nil {
			t.Fatal(err)
		}
		if err := Unmarshal(b, test.out); err != nil {
			t.Fatal(err)
		}
		bb, err := Marshal(test.out)
		if err != nil {
			t.Fatal(err)
		}
		if !bytes.Equal(b, bb) {
			t.Fatal("bytes not equal")
		}
		if test.in.String() != test.out.(Message).String() {
			t.Fatal("strings not equal")
		}
	}

}

func TestEncodeDecode(t *testing.T) {
	tests := []struct {
		in Message
		id uint32
		ok bool
	}{
		{
			in: CloseMessage{
				ChannelID: 10,
			},
			id: 10,
			ok: true,
		},
		{
			in: DataMessage{
				ChannelID: 10,
				Length:    5,
				Data:      []byte("Hello"),
			},
			id: 10,
			ok: true,
		},
		{
			in: EOFMessage{
				ChannelID: 10,
			},
			id: 10,
			ok: true,
		},
		{
			in: OpenMessage{
				SenderID:      10,
				WindowSize:    1024,
				MaxPacketSize: 1 << 31,
			},
			id: 0,
			ok: false,
		},
		{
			in: OpenConfirmMessage{
				ChannelID:     20,
				SenderID:      10,
				WindowSize:    1024,
				MaxPacketSize: 1 << 31,
			},
			id: 20,
			ok: true,
		},
		{
			in: OpenFailureMessage{
				ChannelID: 20,
			},
			id: 20,
			ok: true,
		},
		{
			in: WindowAdjustMessage{
				ChannelID:       20,
				AdditionalBytes: 1024,
			},
			id: 20,
			ok: true,
		},
	}
	for _, test := range tests {
		var buf bytes.Buffer
		enc := NewEncoder(&buf)
		if err := enc.Encode(test.in); err != nil {
			t.Fatal(err)
		}
		dec := NewDecoder(&buf)
		m, err := dec.Decode()
		if err != nil {
			t.Fatal(err)
		}
		id, ok := m.Channel()
		if id != test.id {
			t.Fatal("id not equal")
		}
		if ok != test.ok {
			t.Fatal("ok not equal")
		}
	}

}
