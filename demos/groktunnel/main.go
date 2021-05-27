package main

import (
	"bufio"
	"context"
	"crypto/rand"
	"flag"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"net/http/httputil"
	"strings"
	"time"

	vhost "github.com/inconshreveable/go-vhost"
	"github.com/progrium/qmux/golang/session"
)

func main() {
	var port = flag.String("p", "9999", "server port to use")
	var host = flag.String("h", "vcap.me", "server hostname to use")
	var addr = flag.String("b", "127.0.0.1", "ip to bind [server only]")
	flag.Parse()

	// client usage: groktunnel [-h=<server hostname>] <local port>
	if flag.Arg(0) != "" {
		conn, err := net.Dial("tcp", net.JoinHostPort(*host, *port))
		fatal(err)
		client := httputil.NewClientConn(conn, bufio.NewReader(conn))
		req, err := http.NewRequest("GET", "/", nil)
		req.Host = net.JoinHostPort(*host, *port)
		fatal(err)
		client.Write(req)
		resp, _ := client.Read(req)
		fmt.Printf("port %s http available at:\n", flag.Arg(0))
		fmt.Printf("http://%s\n", resp.Header.Get("X-Public-Host"))
		c, _ := client.Hijack()
		sess := session.New(c)
		defer sess.Close()
		for {
			ch, err := sess.Accept()
			fatal(err)
			conn, err := net.Dial("tcp", "127.0.0.1:"+flag.Arg(0))
			fatal(err)
			go join(conn, ch)
		}
		return
	}

	// server usage: groktunnel [-h=<hostname>] [-b=<bind ip>]
	l, err := net.Listen("tcp", net.JoinHostPort(*addr, *port))
	fatal(err)
	defer l.Close()
	vmux, err := vhost.NewHTTPMuxer(l, 1*time.Second)
	fatal(err)

	go serve(vmux, *host, *port)

	log.Printf("groktunnel server [%s] ready!\n", *host)
	for {
		conn, err := vmux.NextError()
		fmt.Println(err)
		if conn != nil {
			conn.Close()
		}
	}
}

func serve(vmux *vhost.HTTPMuxer, host, port string) {
	ml, err := vmux.Listen(net.JoinHostPort(host, port))
	fatal(err)
	srv := &http.Server{Handler: http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		publicHost := strings.TrimSuffix(net.JoinHostPort(newSubdomain()+host, port), ":80")
		pl, err := vmux.Listen(publicHost)
		fatal(err)
		w.Header().Add("X-Public-Host", publicHost)
		w.Header().Add("Connection", "close")
		w.WriteHeader(http.StatusOK)
		conn, _, _ := w.(http.Hijacker).Hijack()
		sess := session.New(conn)
		defer sess.Close()
		log.Printf("%s: start session", publicHost)
		go func() {
			for {
				conn, err := pl.Accept()
				if err != nil {
					log.Println(err)
					return
				}
				ch, err := sess.Open(context.Background())
				if err != nil {
					log.Println(err)
					return
				}
				go join(ch, conn)
			}
		}()
		sess.Wait()
		log.Printf("%s: end session", publicHost)
	})}
	srv.Serve(ml)
}

func join(a io.ReadWriteCloser, b io.ReadWriteCloser) {
	go io.Copy(b, a)
	io.Copy(a, b)
	a.Close()
	b.Close()
}

func newSubdomain() string {
	b := make([]byte, 10)
	if _, err := rand.Read(b); err != nil {
		panic(err)
	}
	letters := []rune("abcdefghijklmnopqrstuvwxyz1234567890")
	r := make([]rune, 10)
	for i := range r {
		r[i] = letters[int(b[i])*len(letters)/256]
	}
	return string(r) + "."
}

func fatal(err error) {
	if err != nil {
		log.Fatal(err)
	}
}
