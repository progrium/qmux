package codec

import (
	"encoding/binary"
	"fmt"
)

type WindowAdjustMessage struct {
	ChannelID       uint32
	AdditionalBytes uint32
}

func (msg WindowAdjustMessage) String() string {
	return fmt.Sprintf("{WindowAdjustMessage ChannelID:%d AdditionalBytes:%d}",
		msg.ChannelID, msg.AdditionalBytes)
}

func (msg WindowAdjustMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg WindowAdjustMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelWindowAdjust]+1)
	packet[0] = msgChannelWindowAdjust
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	binary.BigEndian.PutUint32(packet[5:9], msg.AdditionalBytes)
	return packet, nil
}

func (msg *WindowAdjustMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	msg.AdditionalBytes = binary.BigEndian.Uint32(b[5:9])
	return nil
}
