package codec

import (
	"encoding/binary"
	"fmt"
)

type CloseMessage struct {
	ChannelID uint32
}

func (msg CloseMessage) String() string {
	return fmt.Sprintf("{CloseMessage ChannelID:%d}", msg.ChannelID)
}

func (msg CloseMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg CloseMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelClose]+1)
	packet[0] = msgChannelClose
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	return packet, nil
}

func (msg *CloseMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	return nil
}
