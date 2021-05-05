
export interface IConn {
    read(len: number): Promise<Uint8Array | undefined>;
    write(buffer: Uint8Array): Promise<number>;
    close(): Promise<void>;
}

export interface ISession {
    open(): Promise<IChannel>;
    accept(): Promise<IChannel | undefined>;
    close(): Promise<void>;
}

export interface IChannel extends IConn {
    ident(): number
    closeWrite(): Promise<void>
}

export interface IConnListener {
    accept(): Promise<IConn | undefined>;
    close(): Promise<void>;
}
