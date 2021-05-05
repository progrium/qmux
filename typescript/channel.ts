// @ts-ignore
import * as util from "./util.ts";
// @ts-ignore
import * as codec from "./codec/index.ts";
// @ts-ignore
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
    ready: util.queue<boolean>;
    sentEOF: boolean;
    sentClose: boolean;
    remoteWin: number;
    myWindow: number;
    readBuf: util.ReadBuffer;
    writers: Array<() => void>;

    constructor(sess: internal.Session) {
        this.localId = 0;
        this.remoteId = 0;
        this.maxIncomingPayload = 0;
        this.maxRemotePayload = 0;
        this.sentEOF = false;
        this.sentClose = false;
        this.remoteWin = 0;
        this.myWindow = 0;
        this.ready = new util.queue();
        this.session = sess;
        this.writers = [];
        this.readBuf = new util.ReadBuffer();
    }

    ident(): number {
        return this.localId;
    }

    async read(len: number): Promise<Uint8Array | undefined> {
        let data = await this.readBuf.read(len);
        if (data !== undefined) {
            try {
                await this.adjustWindow(data.byteLength)
            } catch (e) {
                if (e !== "EOF") {
                    throw e;
                }
            }
        }
        return data;
    }

    reserveWindow(win: number): number {
        if (this.remoteWin < win) {
            win = this.remoteWin;
        }
        this.remoteWin -= win;
        return win;
    }

    addWindow(win: number) {
        this.remoteWin += win;
        while (this.remoteWin > 0) {
            let writer = this.writers.shift();
            if (!writer) break;
            writer();
        }
    }

    write(buffer: Uint8Array): Promise<number> {
        if (this.sentEOF) {
            return Promise.reject("EOF");
        }

        return new Promise((resolve, reject) => {
            let n = 0;
            let tryWrite = () => {
                if (this.sentEOF || this.sentClose) {
                    reject("EOF");
                    return;
                }
                if (buffer.byteLength == 0) {
                    resolve(n);
                    return;
                }
                let space = Math.min(this.maxRemotePayload, buffer.byteLength);
                let reserved = this.reserveWindow(space);
                if (reserved == 0) {
                    this.writers.push(tryWrite);
                    return;
                }

                let toSend = buffer.slice(0, reserved);

                this.send({
                    ID: codec.DataID,
                    channelID: this.remoteId,
                    length: toSend.byteLength,
                    data: toSend,
                }).then(() => {
                    n += toSend.byteLength;
                    buffer = buffer.slice(toSend.byteLength);
                    if (buffer.byteLength == 0) {
                        resolve(n);
                        return;
                    }
                    this.writers.push(tryWrite);
                })
            }
            tryWrite();
        })
    }

    async closeWrite() {
        this.sentEOF = true;
        await this.send({
            ID: codec.EofID,
            channelID: this.remoteId
        });
        this.writers.forEach(writer => writer());
        this.writers = [];
    }

    async close(): Promise<void> {
        if (!this.sentClose) {
            await this.send({
                ID: codec.CloseID,
                channelID: this.remoteId
            });
            this.sentClose = true;
            while (await this.ready.shift() !== undefined) { }
            return;
        }
        this.shutdown();
    }

    shutdown(): void {
        this.readBuf.close();
        this.writers.forEach(writer => writer());
        this.ready.close();
        this.session.rmCh(this.localId);
    }

    async adjustWindow(n: number) {
        // Since myWindow is managed on our side, and can never exceed
        // the initial window setting, we don't worry about overflow.
        this.myWindow += n;
        await this.send({
            ID: codec.WindowAdjustID,
            channelID: this.remoteId,
            additionalBytes: n,
        })
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
            this.readBuf.eof();
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
            this.addWindow(msg.windowSize);
            this.ready.push(true);
            return;
        }
        if (msg.ID === codec.WindowAdjustID) {
            this.addWindow(msg.additionalBytes);
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

        this.readBuf.write(msg.data)
    }

}

