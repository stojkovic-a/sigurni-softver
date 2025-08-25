import { Inject, Injectable, OnModuleInit } from '@nestjs/common';
import { repl } from '@nestjs/core';
import { ClientProvider, ClientProxy, NatsRecordBuilder, NatsStatus } from '@nestjs/microservices';
import { privateDecrypt } from 'crypto';
import { stat } from 'fs';
import { Events } from 'nats';

@Injectable()
export class CacheService {
    constructor(
        @Inject('cache_service') private readonly clientNATS: ClientProxy,
    ) {
    }
    async sendMessage(pattern: string, data: string) {
        try {
            const record = new NatsRecordBuilder(data)
                .build()
            await this.clientNATS
                .emit(pattern, record)
                .subscribe()
        }
        catch (err) {
            throw err;
        }
    }
}

// import { Inject, Injectable, OnModuleDestroy } from '@nestjs/common';
// import { repl } from '@nestjs/core';
// import { ClientProvider, ClientProxy, NatsRecordBuilder } from '@nestjs/microservices';
// import { privateDecrypt } from 'crypto';
// import { NATS_CONNECTION } from './nats.client.provider';
// import { NatsConnection } from 'nats';

// @Injectable()
// export class CacheService implements OnModuleDestroy {
//     constructor(
//         @Inject(NATS_CONNECTION) private readonly natsClient: NatsConnection,
//     ) {
//     }

//     async sendMessage(pattern: string, data: string) {
//         try {
//             console.log("sent that shit")
//             this.natsClient.publish(pattern, data)
//         }
//         catch (err) {
//             console.error("Failed to publish message", err);
//         }
//     }
//     async onModuleDestroy() {
//         await this.natsClient.drain();
//     }
// }
