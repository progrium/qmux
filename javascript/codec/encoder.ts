import * as api from "../api.ts";
import * as msg from "./message.ts";

export class Encoder {
    conn: api.IConn;
    debug: boolean;

    constructor(conn: api.IConn, debug: boolean = false) {
        this.conn = conn;
        this.debug = debug;
    }

    async encode(m: msg.AnyMessage): Promise<number> {
        if (this.debug) {
            console.log("<<", m);
        }
        return this.conn.write(Marshal(m));
    }
}

export function Marshal(obj: msg.AnyMessage): Uint8Array {
    if (obj.ID === msg.CloseID) {
        let m = obj as msg.CloseMessage;
        let data = new DataView(new ArrayBuffer(5));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        return new Uint8Array(data.buffer);
    }
    if (obj.ID === msg.DataID) {
        let m = obj as msg.DataMessage;
        let data = new DataView(new ArrayBuffer(9));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        data.setUint32(5, m.length);
        let buf = new Uint8Array(9+m.length);
		buf.set(new Uint8Array(data.buffer), 0);
		buf.set(m.data, 9);
        return buf;
    }
    if (obj.ID === msg.EofID) {
        let m = obj as msg.EOFMessage;
        let data = new DataView(new ArrayBuffer(5));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        return new Uint8Array(data.buffer);
    }
    if (obj.ID === msg.OpenID) {
        let m = obj as msg.OpenMessage;
        let data = new DataView(new ArrayBuffer(13));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.senderID);
        data.setUint32(5, m.windowSize);
		data.setUint32(9, m.maxPacketSize);
        return new Uint8Array(data.buffer);
    }
    if (obj.ID === msg.OpenConfirmID) {
        let m = obj as msg.OpenConfirmMessage;
        let data = new DataView(new ArrayBuffer(17));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        data.setUint32(5, m.senderID);
		data.setUint32(9, m.windowSize);
		data.setUint32(13, m.maxPacketSize);
        return new Uint8Array(data.buffer);
    }
    if (obj.ID === msg.OpenFailureID) {
        let m = obj as msg.OpenFailureMessage;
        let data = new DataView(new ArrayBuffer(5));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        return new Uint8Array(data.buffer);
    }
    if (obj.ID === msg.WindowAdjustID) {
        let m = obj as msg.WindowAdjustMessage;
        let data = new DataView(new ArrayBuffer(9));
        data.setUint8(0, m.ID);
        data.setUint32(1, m.channelID);
        data.setUint32(5, m.additionalBytes);
        return new Uint8Array(data.buffer);
    }
    throw `marshal of unknown type: ${obj}`;
}