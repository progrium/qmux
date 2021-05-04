import {
    assertEquals,
} from "https://deno.land/std/testing/asserts.ts";

import * as session from "./session.ts";
import * as api from "./api.ts";
import * as util from "./util.ts";
import * as tcp from "./transport/deno/tcp.ts";
import * as websocket from "./transport/deno/websocket.ts";

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

async function startListener(listener: api.IConnListener) {
    let conn = await listener.accept();
    if (!conn) {
        throw new Error("accept failed")
    }
    let sess = new session.Session(conn);
    let ch = await sess.open();
    let b = await readAll(ch);
    await ch.close();

    let ch2 = await sess.accept();
    if (ch2 === undefined) {
        throw new Error("accept failed")
    }
    await ch2.write(b);
    await ch2.close();
    try {
        await sess.close();
        await listener.close();
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
    let listener = new tcp.Listener({ port: 0 });
    let port = (listener.listener.addr as Deno.NetAddr).port;
    await Promise.all([
        startListener(listener),
        tcp.Dial({ port }).then(conn => {
            return testExchange(conn);
        }),
    ]);
});

Deno.test("websocket", async () => {
    let endpoint = "ws://127.0.0.1:9999";
    let listener = new websocket.Listener(9999);
    await Promise.all([
        startListener(listener),
        websocket.Dial(endpoint).then(conn => {
            return testExchange(conn);
        }),
    ]);
});


Deno.test("multiple pending reads", async () => {
    let listener = Deno.listen({ port: 0 });

    let port = (listener.addr as Deno.NetAddr).port;

    let lConn = listener.accept();

    let sess1 = new session.Session(new tcp.Conn(await Deno.connect({ port })));
    let sess2 = new session.Session(new tcp.Conn(await lConn));

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
