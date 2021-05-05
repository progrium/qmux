import { StandardWebSocketClient, WebSocketClient, WebSocketServer } from "https://deno.land/x/websocket@v0.1.2/mod.ts";

// @ts-ignore
import * as api from "./../../api.ts";
// @ts-ignore
import * as internal from "./../../internal.ts";
// @ts-ignore
import * as util from "./../../util.ts";

export class Listener implements api.IConnListener {
    wss: WebSocketServer
    q: util.queue<Conn>

    constructor(port: number) {
        this.q = new util.queue();
        this.wss = new WebSocketServer(port);
        this.wss.on("connection", (ws: WebSocketClient) => {
            this.q.push(new Conn(ws));
        })
    }

    accept(): Promise<Conn | undefined> {
        return this.q.shift();
    }

    async close(): Promise<void> {
        await this.wss.close();
        this.q.close();
    }
}

export function Dial(endpoint: string): Promise<Conn> {
    let ws = new StandardWebSocketClient(endpoint);
    return new Promise<Conn>((resolve) => {
        // TODO errors?
        ws.on("open", function () {
            resolve(new Conn(ws));
        });
    })
}

export class Conn implements api.IConn {
    socket: WebSocketClient
    buf: util.ReadBuffer
    isClosed: boolean

    constructor(socket: WebSocketClient) {
        this.isClosed = false;
        this.socket = socket;
        this.buf = new util.ReadBuffer();
        this.socket.on("message", (event: MessageEvent<Blob> | Uint8Array) => {
            if (event instanceof Uint8Array) {
                this.buf.write(event);
                return;
            }
            event.data.arrayBuffer().then((data) => {
                let buf = new Uint8Array(data);
                this.buf.write(buf);
            });
        });
        this.socket.on("close", () => {
            this.close();
        });
        //this.socket.onerror = (err) => console.error("qtalk", err);
    }

    read(len: number): Promise<Uint8Array | undefined> {
        return this.buf.read(len);
    }

    write(buffer: Uint8Array): Promise<number> {
        this.socket.send(buffer);
        return Promise.resolve(buffer.byteLength);
    }

    async close(): Promise<void> {
        if (this.isClosed) {
            return;
        }
        this.isClosed = true;
        this.buf.close();
        await this.socket.close(1000); // Code 1000: Normal Closure
    }
}
