import { FactoryProvider } from "@nestjs/common";
import Redis from "ioredis";


export const redisClientFactory: FactoryProvider<Redis> = {
    provide: 'RedisClient',
    useFactory: () => {
        const redisInstance = new Redis({
            host: "redis-17414.c328.europe-west3-1.gce.redns.redis-cloud.com",
            port: 17414,
            username: "default",
            password: "jA6I2L38dStyd7c1TW7csaxAswyJMWtD"
        });
        redisInstance.on('error', e => {
            throw new Error(`Redis connection failed: ${e}`);
        });

        return redisInstance;
    },
    inject: [],
};

export const redisPubSubFactory: FactoryProvider<Redis> = {
    provide: 'RedisPubSub',
    useFactory: () => {
        const redisInstance = new Redis({
            host: 'redis-17414.c328.europe-west3-1.gce.redns.redis-cloud.com',
            port: 17414,
            username: "default",
            password: "jA6I2L38dStyd7c1TW7csaxAswyJMWtD"
        });
        return redisInstance;
    },
    inject: [],
}