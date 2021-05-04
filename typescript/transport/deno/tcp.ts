// @ts-ignore
import * as api from "../../api.ts";

export class Conn implements api.IConn {
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
