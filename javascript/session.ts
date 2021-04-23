import * as api from "./api.ts";
import * as codec from "./codec/index.ts";
import * as util from "./util.ts";
import * as internal from "./internal.ts";

export const minPacketLength = 9;
export const maxPacketLength = Number.MAX_VALUE;


export class Session implements api.ISession {
	conn: api.IConn;
	channels: Array<internal.Channel>;
	incoming: util.queue;
	enc: codec.Encoder;
	dec: codec.Decoder;

	constructor(conn: api.IConn, debug: boolean = false) {
		this.conn = conn;
		this.enc = new codec.Encoder(conn, debug);
		this.dec = new codec.Decoder(conn, debug);
		this.channels = [];
		this.incoming = new util.queue();
		this.loop();
	}

	async open(): Promise<api.IChannel> {
		let ch = this.newChannel();
		ch.maxIncomingPayload = internal.channelMaxPacket;
		await this.enc.encode({
			ID: codec.OpenID,
			windowSize: ch.myWindow,
			maxPacketSize: ch.maxIncomingPayload,
			senderID: ch.localId
		});
		if (await ch.ready.shift()) {
			return ch;
		}
		throw "failed to open";
	}

	accept(): Promise<api.IChannel> {
		return this.incoming.shift();
	}

	async close(): Promise<void> {
		for (const ids of Object.keys(this.channels)) {
            let id = parseInt(ids);
			if (this.channels[id] !== undefined) {
				this.channels[id].shutdown();
			}
		}
		return this.conn.close();
	}

	async loop() {
		try {
			while (true) {
				let msg = await this.dec.decode();
				if (msg === undefined) {
					this.close();
					return;
				}
				if (msg.ID === codec.OpenID) {
					await this.handleOpen(msg as codec.OpenMessage);
					continue;
				}
				
				let cmsg: codec.ChannelMessage = msg as codec.ChannelMessage;

				let ch = this.getCh(cmsg.channelID);
				if (ch === undefined) {
					throw `invalid channel (${cmsg.channelID}) on op ${cmsg.ID}`;
				}
				await ch.handle(cmsg);
			}
		} catch (e) {
			throw new Error(`session readloop: ${e}`);
		}
		// catch {
		// 	this.channels.forEach(async (ch) => {
		// 		await ch.close();
		// 	})
		// 	this.channels = [];
		// 	await this.conn.close();
		// }
	}

	async handleOpen(msg: codec.OpenMessage) {
		if (msg.maxPacketSize < minPacketLength || msg.maxPacketSize > maxPacketLength) {
			await this.enc.encode({
				ID: codec.OpenFailureID,
				channelID: msg.senderID
			});
			return;
		}
		let c = this.newChannel();
		c.remoteId = msg.senderID;
		c.maxRemotePayload = msg.maxPacketSize;
		c.remoteWin = msg.windowSize;
		c.maxIncomingPayload = internal.channelMaxPacket;
		this.incoming.push(c);
		await this.enc.encode({
			ID: codec.OpenConfirmID,
			channelID: c.remoteId,
			senderID: c.localId,
			windowSize: c.myWindow,
			maxPacketSize: c.maxIncomingPayload
		});
	}

	newChannel(): internal.Channel {
		let ch = new internal.Channel(this);
		ch.remoteWin = 0;
		ch.myWindow = internal.channelWindowSize;
		ch.readBuf = new Uint8Array(0);
		ch.localId = this.addCh(ch);
		return ch;
	}

	getCh(id: number): internal.Channel {
		let ch = this.channels[id];
		if (ch.localId !== id) {
			console.log("bad ids:", id, ch.localId, ch.remoteId);
		}
		return ch;
	}
	
	addCh(ch: internal.Channel): number {
		this.channels.forEach((v,i) => {
			if (v === undefined) {
				this.channels[i] = ch;
				return i;
			}
		});
		this.channels.push(ch);
		return this.channels.length-1;
	}

	rmCh(id: number): void {
		delete this.channels[id];
	}

}

