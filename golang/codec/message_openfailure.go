package codec

import (
	"encoding/binary"
	"fmt"
)

type OpenFailureMessage struct {
	ChannelID uint32
}

func (msg OpenFailureMessage) String() string {
	return fmt.Sprintf("{OpenFailureMessage ChannelID:%d}", msg.ChannelID)
}

func (msg OpenFailureMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg OpenFailureMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelOpenFailure]+1)
	packet[0] = msgChannelOpenFailure
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	return packet, nil
}

func (msg *OpenFailureMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	return nil
}
