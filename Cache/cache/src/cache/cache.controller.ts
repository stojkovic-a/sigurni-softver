import { Controller, Inject, Injectable, OnApplicationBootstrap, OnModuleInit, ParseArrayPipe } from '@nestjs/common';
import { ClientProxy, Ctx, EventPattern, MessagePattern, NatsContext, NatsRecordBuilder, Payload, Transport } from '@nestjs/microservices';
import { CacheService } from './cache.service';
import * as nats from 'nats';
import { ColdObservable } from 'rxjs/internal/testing/ColdObservable';
import { RedisRepository } from './redis.repository';
import { RedisSubscriberService } from './redis-subscriber.service';
import { Events } from './data/enums/events';
import { Console } from 'console';
import { ConsumerOptsBuilderImpl } from 'nats/lib/jetstream/types';
import { ConfirmationMailPayload, Outbox, StoreMailPayload } from './data/interfaces/outbox';
import path from 'path';
import { EventPayloadMap, EventType } from './data/types/event.types';
import { RequestService } from './request.service';


@Controller('cache')
export class CacheController {
    constructor(
        private readonly redisRepo: RedisRepository,
        private readonly cacheService: CacheService,
        private readonly requestService: RequestService,
    ) {
    }

    async onApplicationBootstrap() {
    }


    // @EventPattern('cache.*', Transport.NATS)
    // async ReceiveTest(@Payload() data, @Ctx() context: NatsContext) {
    //     try {
    //         console.log("aaa");
    //         console.log(data)
    //         this.redisRepo.setWithExpiry("test", "testic", "test2", 10)

    //     } catch (e) {
    //         console.error(e);
    //     }
    // }

    @EventPattern(`cache.${Events[Events.STORE_EMAIL]}`, Transport.NATS)
    async MailStore(@Payload() data: Outbox, @Ctx() context: NatsContext) {
        try {
            const payload: StoreMailPayload = JSON.parse(data.Payload)
            const exists = await this.redisRepo.get("email_confirmation", `${payload.UserId}`)
            if (exists === null) {
                this.redisRepo.setWithExpiry("email_confirmation", `${payload.UserId}`, payload.ConfirmationCode, 2 * 60 * 60)
            }
            this.cacheService.sendMessage(`auth.${Events[Events.STORE_EMAIL]}.${data.Id}.${payload.UserId}`, 'SUCCEEDED')
        }
        catch (e) {
            const payload: ConfirmationMailPayload = JSON.parse(data.Payload)
            this.cacheService.sendMessage(`auth.${Events[Events.STORE_EMAIL]}.${data.Id}.${payload.UserId}`, 'FAILED')
            console.error(e);
        }
    }
    @EventPattern(`cache.${Events[Events.CONFIRM_EMAIL]}`, Transport.NATS)
    async MailConfirm(@Payload() data: Outbox, @Ctx() context: NatsContext) {
        try {
            const payload: ConfirmationMailPayload = JSON.parse(data.Payload)
            console.log(payload)
            const codeDb = await this.redisRepo.get("email_confirmation", `${payload.UserId}`)
            if (codeDb === null || codeDb != payload.ConfirmationCode) {
                this.cacheService.sendMessage(`auth.${Events[Events.CONFIRM_EMAIL]}.${data.Id}.${payload.UserId}`, 'NOTFOUND')
                return;
            }
            const responseData = await this.requestService.createSendRequest(data.Payload, "https://go-client:9998/v1/internal/createProfile")
            if (responseData == 200) {
                this.cacheService.sendMessage(`auth.${Events[Events.CONFIRM_EMAIL]}.${data.Id}.${payload.UserId}`, 'SUCCEEDED')
                return
            }
            this.cacheService.sendMessage(`auth.${Events[Events.CONFIRM_EMAIL]}.${data.Id}.${payload.UserId}`, 'FAILED')

        } catch (e) {
            const payload: ConfirmationMailPayload = JSON.parse(data.Payload)
            this.cacheService.sendMessage(`auth.${Events[Events.CONFIRM_EMAIL]}.${data.Id}.${payload.UserId}`, 'FAILED')
        }
    }


    // async SendTest() {
    //     try {
    //         const data = 'test'
    //         const headers = nats.headers();
    //         headers.set('x-version', '1.0.0');
    //         const record = new NatsRecordBuilder(data)
    //             .setHeaders(headers)
    //             .build();
    //         await this.clientNATS
    //             .emit('cache.*', record)
    //             .subscribe();
    //     } catch (e) {
    //         console.error(e);
    //     }
    // }
}

