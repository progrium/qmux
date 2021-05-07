# qmux

qmux is a wire protocol for multiplexing connections or streams into a single connection. It is based on the [SSH Connection Protocol](https://tools.ietf.org/html/rfc4254#page-5), which is the simplest, longest running, most widely deployed TCP multiplexing protocol with flow control. 

It is meant as a drop-in layer for any stream capable transport (TCP, WebSocket, stdio, etc) to provide basic multiplexing. This brings any connection API to rough semantic parity with [QUIC](https://en.wikipedia.org/wiki/QUIC) multiplexing, so it can act as a stopgap or fallback when QUIC is not available. You can then design higher level protocols based on multiplexing semantics that sit on top of QUIC or any other streaming transport with qmux.

## Spec

The specification is [here](https://github.com/progrium/qmux/blob/main/SPEC.md). It is a simplified version of the [Channel Mechanism](https://tools.ietf.org/html/rfc4254#page-5) in the SSH Connection Protocol.

## Implementations

- [x] [Golang](https://github.com/progrium/qmux/tree/main/golang) (best reference)
- [x] [TypeScript](https://github.com/progrium/qmux/tree/main/typescript)
- [ ] Python
- [ ] C# (help wanted)

## Demos

- [groktunnel](https://github.com/progrium/qmux/tree/main/demos/groktunnel): Ephemeral localhost public forwarding system for HTTP similar to Ngrok in less than 150 lines of Go.

## About

Licensed MIT
