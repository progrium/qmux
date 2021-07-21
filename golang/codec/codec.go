// Package codec implements encoding and decoding of qmux messages.
package codec

import "io"

var (
	DebugMessages io.Writer
	DebugBytes    io.Writer
)
