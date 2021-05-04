
export function concat(list: Uint8Array[], totalLength: number): Uint8Array {
    let buf = new Uint8Array(totalLength);
    let offset = 0;
    list.forEach((el) => {
        buf.set(el, offset);
        offset += el.length;
    });
    return buf;
}

// queue primitive for incoming connections and
// signaling channel ready state
export class queue<ValueType> {
    q: Array<ValueType>
    waiters: Array<(a: ValueType | undefined) => void>
    closed: boolean

    constructor() {
        this.q = [];
        this.waiters = [];
        this.closed = false;
    }

    push(obj: ValueType) {
        if (this.closed) throw "closed queue";
        if (this.waiters.length > 0) {
            let waiter = this.waiters.shift()
            if (waiter) waiter(obj);
            return;
        }
        this.q.push(obj);
    }

    shift(): Promise<ValueType | undefined> {
        if (this.closed) return Promise.resolve(undefined);
        return new Promise(resolve => {
            if (this.q.length > 0) {
                resolve(this.q.shift());
                return;
            }
            this.waiters.push(resolve);
        })
    }

    close() {
        if (this.closed) return;
        this.closed = true;
        this.waiters.forEach(waiter => {
            waiter(undefined);
        });
    }
}

export class ReadBuffer {
    gotEOF: boolean;
    readBuf: Uint8Array | undefined;
    readers: Array<() => void>;

    constructor() {
        this.readBuf = new Uint8Array(0);
        this.gotEOF = false;
        this.readers = [];
    }

    read(len: number): Promise<Uint8Array | undefined> {
        return new Promise(resolve => {
            let tryRead = () => {
                if (this.readBuf === undefined) {
                    resolve(undefined);
                    return;
                }
                if (this.readBuf.length == 0) {
                    if (this.gotEOF) {
                        this.readBuf = undefined;
                        resolve(undefined);
                        return;
                    }
                    this.readers.push(tryRead);
                    return;
                }
                let data = this.readBuf.slice(0, len);
                this.readBuf = this.readBuf.slice(data.byteLength);
                if (this.readBuf.length == 0 && this.gotEOF) {
                    this.readBuf = undefined;
                }
                resolve(data);
            }
            tryRead();
        });
    }

    write(data: Uint8Array) {
        if (this.readBuf) {
            this.readBuf = concat([this.readBuf, data], this.readBuf.length + data.length);
        }

        while (!this.readBuf || this.readBuf.length > 0) {
            let reader = this.readers.shift();
            if (!reader) break
            reader();
        }
    }

    eof() {
        this.gotEOF = true;
        this.flushReaders();
    }

    close() {
        this.readBuf = undefined;
        this.flushReaders();
    }

    protected flushReaders() {
        while (true) {
            let reader = this.readers.shift();
            if (reader === undefined) {
                return;
            }
            reader();
        }
    }
}
