# qmux

qmux is a wire protocol for multiplexing connections or streams into a single connection. It is based on the [SSH Connection Protocol](https://tools.ietf.org/html/rfc4254#page-5), which is the simplest, longest running, most widely deployed multiplexing protocol with flow control. 

It is meant as a drop-in layer for any stream capable transport (TCP, WebSocket, STDIO, etc) to provide basic multiplexing. This brings any connection API to rough semantic parity with QUIC multiplexing, so it can act as a stopgap or fallback when QUIC is not available. You can then design higher level protocols based on multiplexing semantics that sit on top of QUIC or any other streaming transport with qmux.

