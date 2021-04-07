package codec

import (
	"encoding/binary"
	"fmt"
)

type OpenConfirmMessage struct {
	ChannelID     uint32
	SenderID      uint32
	WindowSize    uint32
	MaxPacketSize uint32
}

func (msg OpenConfirmMessage) String() string {
	return fmt.Sprintf("{OpenConfirmMessage ChannelID:%d SenderID:%d WindowSize:%d MaxPacketSize:%d}",
		msg.ChannelID, msg.SenderID, msg.WindowSize, msg.MaxPacketSize)
}

func (msg OpenConfirmMessage) Channel() (uint32, bool) {
	return msg.ChannelID, true
}

func (msg OpenConfirmMessage) MarshalMux() ([]byte, error) {
	packet := make([]byte, payloadSizes[msgChannelOpenConfirm]+1)
	packet[0] = msgChannelOpenConfirm
	binary.BigEndian.PutUint32(packet[1:5], msg.ChannelID)
	binary.BigEndian.PutUint32(packet[5:9], msg.SenderID)
	binary.BigEndian.PutUint32(packet[9:13], msg.WindowSize)
	binary.BigEndian.PutUint32(packet[13:17], msg.MaxPacketSize)
	return packet, nil
}

func (msg *OpenConfirmMessage) UnmarshalMux(b []byte) error {
	msg.ChannelID = binary.BigEndian.Uint32(b[1:5])
	msg.SenderID = binary.BigEndian.Uint32(b[5:9])
	msg.WindowSize = binary.BigEndian.Uint32(b[9:13])
	msg.MaxPacketSize = binary.BigEndian.Uint32(b[13:17])
	return nil
}
