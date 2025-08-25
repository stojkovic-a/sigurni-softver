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


