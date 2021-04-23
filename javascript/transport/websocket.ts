import * as api from "./../api.ts";
import * as internal from "./../internal.ts";
import * as util from "./../util.ts";

export function Dial(addr: string, debug: boolean = false, onclose?: () => void): Promise<api.ISession> {
    return new Promise((resolve) => {
        var socket = new WebSocket(addr);
        socket.onopen = () => resolve(new internal.Session(new Conn(socket), debug));
        //socket.onerror = (err) => console.error("qtalk", err);
        if (onclose) socket.onclose = onclose;
    })
}

export class Conn implements api.IConn {
    socket: WebSocket
    error: any
    waiters: Array<Function>
    buf: Uint8Array;
    isClosed: boolean

    constructor(socket: WebSocket) {
        this.isClosed = false;
        this.buf = new Uint8Array(0);
        this.waiters = [];
        this.socket = socket;
        this.socket.binaryType = "arraybuffer";
        this.socket.onmessage = (event) => {
            var buf = new Uint8Array(event.data);
            this.buf = util.concat([this.buf, buf], this.buf.length+buf.length);
            if (this.waiters.length > 0) {
                let waiter = this.waiters.shift();
                if (waiter) waiter();
            }
        };
        let onclose = this.socket.onclose;
        this.socket.onclose = (e: CloseEvent) => {
            if (onclose) onclose.bind(this.socket)(e);
            this.close();
        }
        //this.socket.onerror = (err) => console.error("qtalk", err);
    }
 
    read(len: number): Promise<Uint8Array|undefined> {
        return new Promise((resolve) => {
            var tryRead = () => {
                if (this.isClosed) {
                    resolve(undefined);
                    return;
                }
                if (this.buf.length >= len) {
                    var data = this.buf.slice(0, len);
                    this.buf = this.buf.slice(len);
                    resolve(data);
                    return;
                }
                this.waiters.push(tryRead);
            }
            tryRead();
        })
    }

    write(buffer: Uint8Array): Promise<number> {
        this.socket.send(buffer);
        return Promise.resolve(buffer.byteLength);
    }

	close(): Promise<void> {
        if (this.isClosed) return Promise.resolve();
        return new Promise((resolve) => {
            this.isClosed = true;
            this.waiters.forEach(waiter => waiter());
            this.socket.close();
            resolve();
        });
    }
}