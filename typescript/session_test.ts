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
        let n: number | null;
        try {
            n = await this.conn.read(buff);
        } catch (e) {
            if (e instanceof Deno.errors.Interrupted || e instanceof Deno.errors.BadResource) {
                return undefined;
            }
            throw e;
        }
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
        try {
            this.conn.close();
        } catch (e) {
            if (!(e instanceof Deno.errors.BadResource)) {
                throw e;
            }
        }
        return Promise.resolve();
    }
}

async function readAll(conn: api.IConn): Promise<Uint8Array> {
    let buff = new Uint8Array();
    while (true) {
        let next = await conn.read(100);
        if (next === undefined) {
            return buff;
        }
        buff = util.concat([buff, next], buff.byteLength + next.byteLength);
    }
}

async function startListener(conn: api.IConn) {
    let sess = new session.Session(conn);
    let ch = await sess.open();
    let b = await readAll(ch);
    await ch.close();

    let ch2 = await sess.accept();
    if (ch2 === undefined) {
        throw new Error("accept failed")
    }
    await ch2.write(b);
    await ch2.closeWrite();
    try {
        await sess.close();
    } catch (e) {
        console.log(e);
    }
}

async function testExchange(conn: api.IConn) {
    let sess = new session.Session(conn);
    let ch = await sess.accept();
    if (ch === undefined) {
        throw new Error("accept failed")
    }

    await ch.write(new TextEncoder().encode("Hello world"));
    await ch.closeWrite();
    await ch.close();

    let ch2 = await sess.open();
    let b = await readAll(ch2);
    await ch2.close();

    assertEquals(new TextEncoder().encode("Hello world"), b);
    try {
        await sess.close();
    } catch (e) {
        console.log(e);
    }
}

Deno.test("tcp", async () => {
    let listener = Deno.listen({ port: 0 });

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
});

Deno.test("multiple pending reads", async () => {
    let listener = Deno.listen({ port: 0 });

    let port = (listener.addr as Deno.NetAddr).port;

    let lConn = listener.accept();

    // let conn1 = Deno.connect({ port });
    let sess1 = new session.Session(new Conn(await Deno.connect({ port })));
    let sess2 = new session.Session(new Conn(await lConn));

    let ch1p = sess1.accept();
    let ch2 = await sess2.open();
    let ch1 = await ch1p;
    if (ch1 === undefined) {
        throw new Error("accept failed");
    }

    let a = ch1.read(1);
    let bc = ch1.read(2);

    await ch2.write(new TextEncoder().encode("abc"));

    assertEquals(await a, new TextEncoder().encode("a"))
    assertEquals(await bc, new TextEncoder().encode("bc"))

    await ch2.closeWrite();
    await ch2.close();
    await sess2.close();

    await ch1.close();
    await sess1.close();

    listener.close();
});
