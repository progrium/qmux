
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
export class queue {
	q: Array<any>
	waiters: Array<Function>
	closed: boolean

	constructor() {
		this.q = [];
		this.waiters = [];
		this.closed = false;
	}

	push(obj: any) {
		if (this.closed) throw "closed queue";
		if (this.waiters.length > 0) {
            let waiter = this.waiters.shift()
            if (waiter) waiter(obj);
			return;
		}
		this.q.push(obj);
	}

	shift(): Promise<any> {
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