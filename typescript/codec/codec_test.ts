import {
    assertEquals,
} from "https://deno.land/std/testing/asserts.ts";

// @ts-ignore
import * as codec from "./index.ts";
import * as msg from "./message.ts";

Deno.test("hello world #1", () => {
    let packet = new Uint8Array(5);
    packet.set([105, 0, 0, 0, 0]);
    let obj = codec.Unmarshal(packet) as msg.AnyMessage;
    let buf = codec.Marshal(obj);
    console.log("Hello", obj, buf);
});
