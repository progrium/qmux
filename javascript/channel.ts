
import * as util from "./util.ts";
import * as codec from "./codec/index.ts";
import * as internal from "./internal.ts";

export const channelMaxPacket = 1 << 15;
export const channelWindowSize = 64 * channelMaxPacket;

// channel represents a virtual muxed connection
export class Channel {
	localId: number;
	remoteId: number;
	maxIncomingPayload: number;
	maxRemotePayload: number;
	session: internal.Session;
	ready: util.queue;
	sentEOF: boolean;
	gotEOF: boolean;
	sentClose: boolean;
	remoteWin: number;
	myWindow: number;
	readBuf: Uint8Array|undefined;
	readers: Array<Function>;

    constructor(sess: internal.Session) {
        this.localId = 0;
        this.remoteId = 0;
        this.maxIncomingPayload = 0;
        this.maxRemotePayload = 0;
        this.sentEOF = false;
        this.gotEOF = false;
        this.sentClose = false;
        this.remoteWin = 0;
        this.myWindow = 0;
        this.ready = new util.queue();
        this.session = sess;
        this.readers = [];
    }

	ident(): number {
		return this.localId;
	}

	read(len: number): Promise<Uint8Array|undefined> {
		return new Promise(resolve => {
			let tryRead = () => {
                if (this.readBuf === undefined) {
                    resolve(undefined);
                    return;
                }
                if (this.readBuf.length >= len) {
                    let data = this.readBuf.slice(0, len);
					this.readBuf = this.readBuf.slice(len);
					resolve(data);
					if (this.readBuf.length == 0 && this.gotEOF) {
						this.readBuf = undefined;
					}
                    return;
				}
                this.readers.push(tryRead);
            }
			tryRead();
		});
	}

	write(buffer: Uint8Array): Promise<number> {
		if (this.sentEOF) {
			return Promise.reject("EOF");
		}
		// TODO: use window 

		return this.send({
			ID: codec.DataID,
			channelID: this.remoteId,
			length: buffer.byteLength,
			data: buffer
		});
	}

	async closeWrite() {
		this.sentEOF = true;
		await this.send({
			ID: codec.EofID,
			channelID: this.remoteId
		});
	}

	async close(): Promise<void> {
		if (!this.sentClose) {
			await this.send({
				ID: codec.CloseID,
				channelID: this.remoteId
			});
			this.sentClose = true;
			while (await this.ready.shift() !== undefined) {}
			return;
		}
		this.shutdown();
	}

	shutdown(): void {
		this.readBuf = undefined;
		this.readers.forEach(reader => reader());
		this.ready.close();
		this.session.rmCh(this.localId);
	}

	async adjustWindow(n: number) {
		// TODO
	}

	send(msg: codec.ChannelMessage): Promise<number> {
		if (this.sentClose) {
			throw "EOF";
		}

		this.sentClose = (msg.ID === codec.CloseID);
		
		return this.session.enc.encode(msg);
	}

	handle(msg: codec.ChannelMessage): void {
		if (msg.ID === codec.DataID) {
			this.handleData(msg as codec.DataMessage);
			return; 
		}
		if (msg.ID === codec.CloseID) {
			this.close(); // is this right?
			return;
		}
		if (msg.ID === codec.EofID) {
			this.gotEOF = true;
			// if (this.readers.length > 0) {
			// 	this.readers.shift()();
			// }
			return;
		}
		if (msg.ID === codec.OpenFailureID) {
			this.session.rmCh(msg.channelID);
			this.ready.push(false);
			return;
		}
		if (msg.ID === codec.OpenConfirmID) {
			if (msg.maxPacketSize < internal.minPacketLength || msg.maxPacketSize > internal.maxPacketLength) {
				throw "invalid max packet size";
			}
			this.remoteId = msg.senderID;
			this.maxRemotePayload = msg.maxPacketSize;
			this.remoteWin += msg.windowSize;
			this.ready.push(true);
			return;
		}
		if (msg.ID === codec.WindowAdjustID) {
			this.remoteWin += msg.additionalBytes;
		}
	}

	handleData(msg: codec.DataMessage) {
		if (msg.length > this.maxIncomingPayload) {
			throw "incoming packet exceeds maximum payload size";
		}

		// TODO: check packet length
		if (this.myWindow < msg.length) {
			throw "remote side wrote too much";
		}
		
		this.myWindow -= msg.length;
		
		if (this.readBuf) {
            this.readBuf = util.concat([this.readBuf, msg.data], this.readBuf.length+msg.data.length);
		}
		
		if (this.readers.length > 0) {
            let reader = this.readers.shift();
            if (reader) reader();
		}
	}
	
}

