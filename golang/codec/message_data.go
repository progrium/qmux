package codec

import (
	"encoding/binary"
	"fmt"
)

type DataMessage struct {
	ChannelID uint32
	Length    uint32
	Data      []byte
}

func (msg DataMessage) String() string {
	return fmt.Sprintf("{DataMessage ChannelID:%d Length:%d Data: ... }",
		msg.ChannelID, msg.Length)
}

func (msg DataMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg DataMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelData]+1)
	packet[0] = msgChannelData
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	binary.BigEndian.PutUint32(packet[5:9], msg.Length)
	return append(packet, msg.Data...), nil
}

func (msg *DataMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	msg.Length = binary.BigEndian.Uint32(b[5:9])
	msg.Data = b[9:]
	return nil
}
