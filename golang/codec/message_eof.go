package codec

import (
	"encoding/binary"
	"fmt"
)

type EOFMessage struct {
	ChannelID uint32
}

func (msg EOFMessage) String() string {
	return fmt.Sprintf("{EOFMessage ChannelID:%d}", msg.ChannelID)
}

func (msg EOFMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg EOFMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelEOF]+1)
	packet[0] = msgChannelEOF
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	return packet, nil
}

func (msg *EOFMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	return nil
}
