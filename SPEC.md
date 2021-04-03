# qmux

qmux is a wire protocol for multiplexing connections or streams into a single connection.
It is a subset of the [SSH Connection Protocol](https://tools.ietf.org/html/rfc4254#page-5).

Features removed to simplify include channel types, channel requests, and "extended data"
messages that were used for STDERR data.

## Channels

   Either side may open a channel.  Multiple channels are multiplexed
   into a single connection.

   Channels are identified by numbers at each end.  The number referring
   to a channel may be different on each side.  Requests to open a
   channel contain the sender's channel number.  Any other channel-
   related messages contain the recipient's channel number for the
   channel.

   Channels are flow-controlled.  No data may be sent to a channel until
   a message is received to indicate that window space is available.

###  Opening a Channel

   When either side wishes to open a new channel, it allocates a local
   number for the channel.  It then sends the following message to the
   other side, and includes the local channel number and initial window
   size in the message.

      byte      QMUX_MSG_CHANNEL_OPEN
      uint32    sender channel
      uint32    initial window size
      uint32    maximum packet size

   The 'sender channel' is a local identifier for the channel used by the
   sender of this message.  The 'initial window size' specifies how many
   bytes of channel data can be sent to the sender of this message
   without adjusting the window. The 'maximum packet size' specifies the
   maximum size of an individual data packet that can be sent to the
   sender.  For example, one might want to use smaller packets for
   interactive connections to get better interactive response on slow
   links.

   The remote side then decides whether it can open the channel, and
   responds with either `QMUX_MSG_CHANNEL_OPEN_CONFIRMATION` or
   `QMUX_MSG_CHANNEL_OPEN_FAILURE`.

      byte      QMUX_MSG_CHANNEL_OPEN_CONFIRMATION
      uint32    recipient channel
      uint32    sender channel
      uint32    initial window size
      uint32    maximum packet size

   The 'recipient channel' is the channel number given in the original
   open request, and 'sender channel' is the channel number allocated by
   the other side.

      byte      QMUX_MSG_CHANNEL_OPEN_FAILURE
      uint32    recipient channel

###  Data Transfer

   The window size specifies how many bytes the other party can send
   before it must wait for the window to be adjusted.  Both parties use
   the following message to adjust the window.

      byte      QMUX_MSG_CHANNEL_WINDOW_ADJUST
      uint32    recipient channel
      uint32    bytes to add

   After receiving this message, the recipient MAY send the given number
   of bytes more than it was previously allowed to send; the window size
   is incremented.  Implementations MUST correctly handle window sizes
   of up to 2^32 - 1 bytes.  The window MUST NOT be increased above
   2^32 - 1 bytes.

   Data transfer is done with messages of the following type.

      byte      QMUX_MSG_CHANNEL_DATA
      uint32    recipient channel
      string    data

   The maximum amount of data allowed is determined by the maximum
   packet size for the channel, and the current window size, whichever
   is smaller.  The window size is decremented by the amount of data
   sent.  Both parties MAY ignore all extra data sent after the allowed
   window is empty.

   Implementations are expected to have some limit on the transport
   layer packet size.

###  Closing a Channel

   When a party will no longer send more data to a channel, it SHOULD
   send `QMUX_MSG_CHANNEL_EOF`.

      byte      QMUX_MSG_CHANNEL_EOF
      uint32    recipient channel

   No explicit response is sent to this message.  However, the
   application may send EOF to whatever is at the other end of the
   channel.  Note that the channel remains open after this message, and
   more data may still be sent in the other direction.  This message
   does not consume window space and can be sent even if no window space
   is available.

   When either party wishes to terminate the channel, it sends
   `QMUX_MSG_CHANNEL_CLOSE`.  Upon receiving this message, a party MUST
   send back an `QMUX_MSG_CHANNEL_CLOSE` unless it has already sent this
   message for the channel.  The channel is considered closed for a
   party when it has both sent and received `QMUX_MSG_CHANNEL_CLOSE`, and
   the party may then reuse the channel number.  A party MAY send
   `QMUX_MSG_CHANNEL_CLOSE` without having sent or received
   `QMUX_MSG_CHANNEL_EOF`.

      byte      QMUX_MSG_CHANNEL_CLOSE
      uint32    recipient channel

   This message does not consume window space and can be sent even if no
   window space is available.

   It is RECOMMENDED that all data sent before this message be delivered
   to the actual destination, if possible.

## Summary of Message Numbers

   The following is a summary of messages and their associated message
   number byte value.

            QMUX_MSG_CHANNEL_OPEN                    100
            QMUX_MSG_CHANNEL_OPEN_CONFIRMATION       101
            QMUX_MSG_CHANNEL_OPEN_FAILURE            102
            QMUX_MSG_CHANNEL_WINDOW_ADJUST           103
            QMUX_MSG_CHANNEL_DATA                    104
            QMUX_MSG_CHANNEL_EOF                     105
            QMUX_MSG_CHANNEL_CLOSE                   106

## Data Type Representations Used

   byte

      A byte represents an arbitrary 8-bit value (octet).  Fixed length
      data is sometimes represented as an array of bytes, written
      byte[n], where n is the number of bytes in the array.

   uint32

      Represents a 32-bit unsigned integer.  Stored as four bytes in the
      order of decreasing significance (network byte order).  For
      example: the value 699921578 (0x29b7f4aa) is stored as 29 b7 f4
      aa.

   string

      Arbitrary length binary string.  Strings are allowed to contain
      arbitrary binary data, including null characters and 8-bit
      characters.  They are stored as a uint32 containing its length
      (number of bytes that follow) and zero (= empty string) or more
      bytes that are the value of the string.  Terminating null
      characters are not used.