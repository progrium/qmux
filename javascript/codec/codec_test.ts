import {
    assertEquals,
} from "https://deno.land/std/testing/asserts.ts";

import * as codec from "./index.ts";

Deno.test("hello world #1", () => {
    let packet = new Uint8Array(5);
    packet.set([105, 0, 0, 0, 0]);
    let obj = codec.Decode(packet);
    let buf = codec.Encode(obj);
    console.log("Hello", obj, buf);
    
});
  