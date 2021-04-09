package codec

import (
	"encoding/binary"
	"fmt"
)

type OpenMessage struct {
	SenderID      uint32
	WindowSize    uint32
	MaxPacketSize uint32
}

func (msg OpenMessage) String() string {
	return fmt.Sprintf("{OpenMessage SenderID:%d WindowSize:%d MaxPacketSize:%d}",
		msg.SenderID, msg.WindowSize, msg.MaxPacketSize)
}

func (msg OpenMessage) Channel() (uint32, bool) {
	return 0, false
}

func (msg OpenMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelOpen]+1)
	packet[0] = msgChannelOpen
	binary.BigEndian.PutUint32(packet[1:5], msg.SenderID)
	binary.BigEndian.PutUint32(packet[5:9], msg.WindowSize)
	binary.BigEndian.PutUint32(packet[9:13], msg.MaxPacketSize)
	return packet, nil
}

func (msg *OpenMessage) UnmarshalMux(b []byte) error {
	msg.SenderID = binary.BigEndian.Uint32(b[1:5])
	msg.WindowSize = binary.BigEndian.Uint32(b[5:9])
	msg.MaxPacketSize = binary.BigEndian.Uint32(b[9:13])
	return nil
}
