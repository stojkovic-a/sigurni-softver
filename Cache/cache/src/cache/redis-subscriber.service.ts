import { Inject, Injectable, OnModuleInit } from "@nestjs/common";
import Redis from "ioredis";
import { CallbackDict } from "./data/interfaces/callback.dictionary";

@Injectable()
export class RedisSubscriberService implements OnModuleInit {
    private callbackDict: CallbackDict;
    constructor(
        @Inject('RedisClient') private readonly redisClient: Redis,
        @Inject('RedisPubSub') private readonly subscriber: Redis,
    ) { }
    async onModuleInit() {
        await this.redisClient.config('SET', 'notify-keyspace-events', 'Ex')
        const db = 0;
        await this.subscriber.subscribe(`__keyevent@${db}__:expired`)


        // await this.publisher.config('SET', 'notify-keyspace-events', 'Ex')

        // const db = 0;
        // const expiredChannel = `__keyevent@${db}__:expired`;

        // await this.subscriber.subscribe(expiredChannel);


        this.callbackDict = {
            "email_confirmation": (param) => {
                console.log(`Email confirmation code has expired for user ${param}`)
            }
        }
        this.subscriber.on('message', (channel, message) => {
            const key = message.split(":")[0]
            const userId = message.split(":")[1]
            this.callbackDict[key](userId)
        })
    }
}