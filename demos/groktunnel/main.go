package main

import (
	"bufio"
	"context"
	"flag"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"net/http/httputil"
	"strings"
	"sync"
	"time"

	vhost "github.com/inconshreveable/go-vhost"
	shortuuid "github.com/lithammer/shortuuid/v3"
	"github.com/progrium/qmux/golang/session"
)

func main() {
	var port = flag.String("p", "9999", "server port to use")
	var host = flag.String("h", "vcap.me", "server hostname to use")
	var addr = flag.String("b", "127.0.0.1", "ip to bind [server only]")
	flag.Parse()

	// client usage: groktunnel -h <server hostname> <local port>
	if flag.Arg(0) != "" {
		conn, err := net.Dial("tcp", "_new."+net.JoinHostPort(*host, *port))
		fatal(err)
		client := httputil.NewClientConn(conn, bufio.NewReader(conn))
		req, err := http.NewRequest("GET", "/", nil)
		req.Host = "_new." + net.JoinHostPort(*host, *port)
		fatal(err)
		client.Write(req)
		resp, err := client.Read(req)
		fatal(err)
		fmt.Printf("port %s http available at:\nhttp://%s\n", flag.Arg(0), resp.Header.Get("X-Public-Host"))
		c, _ := client.Hijack()
		sess := session.New(c)
		for {
			ch, err := sess.Accept()
			fatal(err)
			conn, err := net.Dial("tcp", "127.0.0.1:"+flag.Arg(0))
			fatal(err)
			go join(conn, ch)
		}
		return
	}

	// server usage: groktunnel -h <hostname> -b <bind ip>
	l, err := net.Listen("tcp", net.JoinHostPort(*addr, *port))
	fatal(err)
	vmux, err := vhost.NewHTTPMuxer(l, 1*time.Second)
	fatal(err)
	go func() {
		ml, err := vmux.Listen("_new." + net.JoinHostPort(*host, *port))
		fatal(err)
		mux := http.NewServeMux()
		srv := &http.Server{Handler: mux}
		var sess *session.Session
		mux.HandleFunc(fmt.Sprintf("_new.%s/", *host), func(w http.ResponseWriter, r *http.Request) {
			publicHost := strings.ToLower(fmt.Sprintf("%s.%s", shortuuid.New(), *host))
			pl, err := vmux.Listen(strings.TrimSuffix(net.JoinHostPort(publicHost, *port), ":80"))
			fatal(err)
			go func() {
				for {
					conn, err := pl.Accept()
					if err != nil {
						log.Println(err)
						return
					}
					log.Printf("%s: tunnel conn", publicHost)
					ch, err := sess.Open(context.Background())
					if err != nil {
						log.Println(err)
						return
					}
					go join(conn, ch)
				}
			}()
			w.Header().Add("X-Public-Host", strings.TrimSuffix(net.JoinHostPort(publicHost, *port), ":80"))
			w.Header().Add("Connection", "close")
			w.WriteHeader(http.StatusOK)
			conn, _, _ := w.(http.Hijacker).Hijack()
			sess = session.New(conn)
			log.Printf("%s: new session", publicHost)
			sess.Wait()
		})
		srv.Serve(ml)
	}()

	log.Println("groktunnel server ready!")
	for {
		conn, err := vmux.NextError()
		fmt.Println(err)
		if conn != nil {
			conn.Close()
		}
	}
}

func join(a io.ReadWriteCloser, b io.ReadWriteCloser) {
	var wg sync.WaitGroup
	wg.Add(2)
	go func() {
		io.Copy(a, b)
		wg.Done()
	}()
	go func() {
		io.Copy(b, a)
		wg.Done()
	}()
	wg.Wait()
	a.Close()
	b.Close()
}

func fatal(err error) {
	if err != nil {
		log.Fatal(err)
	}
}
