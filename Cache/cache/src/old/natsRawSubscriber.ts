import { Injectable, OnModuleInit } from "@nestjs/common";
import { connect } from "nats";

@Injectable()
export class NatsRawSubscriber implements OnModuleInit {
    async onModuleInit() {
        const nc = await connect({ servers: 'nats://localhost:4222' });
        const sub = nc.subscribe('test_receive');
        (async () => {
            for await (const m of sub) {
                console.log("raw nats message: ", m.data.toString());
            }
        })();
    }
}