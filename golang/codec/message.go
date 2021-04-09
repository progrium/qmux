package codec

const (
	msgChannelOpen = iota + 100
	msgChannelOpenConfirm
	msgChannelOpenFailure
	msgChannelWindowAdjust
	msgChannelData
	msgChannelEOF
	msgChannelClose
)

var (
	payloadSizes = map[byte]int{
		msgChannelOpen:         12,
		msgChannelOpenConfirm:  16,
		msgChannelOpenFailure:  4,
		msgChannelWindowAdjust: 8,
		msgChannelData:         8,
		msgChannelEOF:          4,
		msgChannelClose:        4,
	}
)

type Message interface {
	Channel() (uint32, bool)
	String() string
}
