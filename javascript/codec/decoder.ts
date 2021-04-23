import * as msg from "./message.ts";
import * as api from "../api.ts";
import * as util from "../util.ts";

export class Decoder {
    conn: api.IConn;
    debug: boolean;

    constructor(conn: api.IConn, debug: boolean = false) {
        this.conn = conn;
        this.debug = debug;
    }

    async decode(): Promise<msg.Message|undefined> {
        let packet = await readPacket(this.conn);
        if (packet === undefined) {
            return Promise.resolve(undefined);
        }
        let msg = Unmarshal(packet);
        if (this.debug) {
            console.log(">>", msg);
        }
        return msg;
    }
}

async function readPacket(conn: api.IConn): Promise<Uint8Array|undefined> {
    let head = await conn.read(1);
    if (head === undefined) {
        return Promise.resolve(undefined);
    }
    let msgID = head[0];
    
    let size = msg.payloadSizes.get(msgID);
    if (size === undefined || msgID < msg.OpenID || msgID > msg.CloseID) {
        return Promise.reject(`bad packet: ${msgID}`);
    }

    let rest = await conn.read(size);
    if (rest === undefined) {
        return Promise.reject("unexpected EOF");
    }

    if (msgID === msg.DataID) {
        let view = new DataView(rest.buffer);
        let length = view.getUint32(4);
        let data = await conn.read(length);
        if (data === undefined) {
            return Promise.reject("unexpected EOF");
        }
        return util.concat([head, rest, data], length+rest.length+1);
    }
    
    return util.concat([head, rest], rest.length+1);
}

export function Unmarshal(packet: Uint8Array): msg.Message {
    let data = new DataView(packet.buffer);
	switch (packet[0]) {
		case msg.CloseID:
			return {
                ID: packet[0],
				channelID: data.getUint32(1)
			} as msg.CloseMessage;
		case msg.DataID:
            let dataLength = data.getUint32(5);
            let rest = new Uint8Array(packet.buffer.slice(9));
			return {
                ID: packet[0],
				channelID: data.getUint32(1),
				length: dataLength,
				data: rest,
			} as msg.DataMessage;
		case msg.EofID:
			return {
                ID: packet[0],
				channelID: data.getUint32(1)
			} as msg.EOFMessage;
		case msg.OpenID:
			return {
                ID: packet[0],
				senderID: data.getUint32(1),
				windowSize: data.getUint32(5),
				maxPacketSize: data.getUint32(9),
			} as msg.OpenMessage;
		case msg.OpenConfirmID:
			return {
                ID: packet[0],
				channelID: data.getUint32(1),
				senderID: data.getUint32(5),
				windowSize: data.getUint32(9),
				maxPacketSize: data.getUint32(13),
			} as msg.OpenConfirmMessage;
		case msg.OpenFailureID:
			return {
                ID: packet[0],
				channelID: data.getUint32(1),
			} as msg.OpenFailureMessage;
		case msg.WindowAdjustID:
			return {
                ID: packet[0],
				channelID: data.getUint32(1),
				additionalBytes: data.getUint32(5),
			} as msg.WindowAdjustMessage;
		default:
			throw `unmarshal of unknown type: ${packet[0]}`;
	}
}
