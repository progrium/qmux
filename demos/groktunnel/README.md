# groktunnel

Expose localhost HTTP servers with a public URL

## Build
```
$ go build
```

## Try it out

First we run the groktunnel server. Normally this would be run on a server, but by default uses `vcap.me`
for a hostname which resolves all of its subdomains to localhost.
```
$ ./groktunnel
2021/04/29 16:10:35 groktunnel server [vcap.me] ready!
```

Now run a local web server. Here is how to run a server listing a file directory with Python:
```
$ python -m SimpleHTTPServer
Serving HTTP on 0.0.0.0 port 8000 ...
```

Then we run groktunnel as a client by giving it the local port to expose.
```
$ ./groktunnel 8000
port 8000 http available at:
http://y8eyshnpol.vcap.me:9999
```

That address should serve the same content as the local web server on 8000. For added effect,
run both client and server with `-p 80`, which will require root to run the server.

## About

This uses qmux between the client and server to tunnel subdomain requests down to the client.
This is done over a hijacked http connection after a tunnel is established and a new subdomain
vhost is setup. 

Not counting dependencies, this whole system is done in under 150 lines. This is the 5th or 6th
implementation of this system since the original [localtunnel](https://github.com/progrium/localtunnel)
in 2010, which was then cloned many times. It was then commercialized by [Ngrok](https://ngrok.com/).