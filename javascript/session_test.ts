import {
    assertEquals,
} from "https://deno.land/std/testing/asserts.ts";
import {
    Buffer,
} from "https://deno.land/std/io/mod.ts";

// @ts-ignore
import * as channel from "./channel.ts";
import * as session from "./session.ts";
import * as api from "./api.ts";
import * as util from "./util.ts";

class Conn implements api.IConn {
    conn: Deno.Conn;

    constructor(conn: Deno.Conn) {
        this.conn = conn;
    }

    async read(len: number): Promise<Uint8Array | undefined> {
        let buff = new Uint8Array(len);
        // TODO does this need to loop if we read less than the expected amount?
        let n = await this.conn.read(buff);
        if (n == null) {
            return undefined;
        }
        if (buff.byteLength > n) {
            buff = buff.slice(0, n);
        }
        return buff;
    }

    write(buffer: Uint8Array): Promise<number> {
        return this.conn.write(buffer)
    }

    close(): Promise<void> {
        // XXX this errors with:
        // BadResource: Bad resource ID if the connection was already closed
        // because the other end disconnected. Should we catch it here, or
        // let that error bubble up?
        this.conn.close();
        return Promise.resolve();
    }
}

async function readAll(conn: api.IConn): Promise<Uint8Array> {
    let buff = new Uint8Array();
    while (true) {
        let next = await conn.read(100);
        console.log("readAll got", next)
        if (next === undefined) {
            return buff;
        }
        buff = util.concat([buff, next], buff.byteLength + next.byteLength);
    }
}

async function startListener(conn: api.IConn) {
    let sess = new session.Session(conn);
    let ch = await sess.open();
    console.log("1 opened")
    let b = await readAll(ch);
    console.log("1 readAll")
    await ch.close();
    console.log("1 closed")

    let ch2 = await sess.accept();
    console.log("1 accepted")
    if (ch2 === undefined) {
        throw new Error("accept failed")
    }
    await ch2.write(b);
    console.log("1 wrote")
    await ch2.closeWrite();
    console.log("1 closed")
    try {
        await sess.close();
        console.log("1 sess.close")
        await sess.done;
        console.log("1 sess.done")
    } catch (e) {
        console.log(e);
    }
}

async function testExchange(conn: api.IConn) {
    let sess = new session.Session(conn);
    let ch = await sess.accept();
    console.log("2 accepted")
    if (ch === undefined) {
        throw new Error("accept failed")
    }

    await ch.write(new TextEncoder().encode("Hello world"));
    console.log("2 wrote")
    await ch.closeWrite();
    console.log("2 closewrite")
    await ch.close();
    console.log("2 closed")

    let ch2 = await sess.open();
    console.log("2 opened")
    let b = await readAll(ch2);
    console.log("2 readAll")
    await ch2.close();
    console.log("2 closed")

    assertEquals(new TextEncoder().encode("Hello world"), b);
    try {
        // XXX this produces BadResourceID when the underlying socket is closed?
        await sess.close();
        console.log("2 sess.close")
        await sess.done;
        console.log("2 sess.done")
    } catch (e) {
        console.log(e);
    }
}

Deno.test("tcp", async () => {
    let listener = Deno.listen({ port: 0 });
    console.log(listener.addr);

    let port = (listener.addr as Deno.NetAddr).port;

    await Promise.all([
        Deno.connect({ port }).then(conn => {
            return startListener(new Conn(conn));
        }),
        listener.accept().then(conn => {
            return testExchange(new Conn(conn));
        }),
    ]);
    listener.close();
})
