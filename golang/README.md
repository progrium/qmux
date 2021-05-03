# qmux for Go

An implementation of qmux for multiplexing any reliable `io.ReadWriteCloser`.

## Using qmux in Go

```
go get github.com/progrium/qmux/golang
```

You can create a qmux session from any `io.ReadWriteCloser` with `session.New`:

```go
package main

import (
    "net"
    "io"
    "context"

    "github.com/progrium/qmux/golang/session"
)

func main() {
    conn, err := net.Dial("tcp", "localhost:9999")
    if err != nil {
        panic(err)
    }
    
    sess := session.New(conn)
    defer sess.Close() // closes underlying conn
    
    ch, err := sess.Open(context.Background())
    if err != nil {
        panic(err)
    }
    defer ch.Close()

    io.WriteString(ch, "Hello world\n")
}

```

However it can be convenient to use the builtin transport dialers and listeners. 

```go
package main

import (
    "io/ioutil"

    "github.com/progrium/qmux/golang/transport"
)

func main() {
    t, err := transport.ListenTCP("localhost:9999")
    if err != nil {
        panic(err)
    }
    defer t.Close()
    
    sess, err := t.Accept()
    if err != nil {
        panic(err)
    }
    defer sess.Close()
    
    ch, err := sess.Accept()
    if err != nil {
        panic(err)
    }
    defer ch.Close()
    
    b, err := ioutil.ReadAll(ch)
    if err != nil {
        panic(err)
    }
    os.Stdout.Write(b) // "Hello world\n" if connected with earlier program
}

```