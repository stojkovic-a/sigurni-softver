import { Module } from '@nestjs/common';
import { CacheController } from './cache.controller';
import { CacheService } from './cache.service';
import { ClientsModule, Transport } from '@nestjs/microservices';
import { NatsRawSubscriber } from '../old/natsRawSubscriber';
import { readFileSync } from 'fs';
import { ConfigModule, ConfigService } from '@nestjs/config';
import configuration from 'src/config/configuration';
import { RedisRepository } from './redis.repository';
import { RedisSubscriberService } from './redis-subscriber.service';
import { redisClientFactory, redisPubSubFactory } from './redis.clientFactory';
import { JwtModule } from '@nestjs/jwt';
import { RequestService } from './request.service';


@Module({
  imports: [ConfigModule.forRoot({
    isGlobal: true,
    load: [configuration]
  }),
  ClientsModule.registerAsync([
    {
      name: "cache_service",
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => {
        const natsOptions = configService.get('nats')
        return {

          transport: Transport.NATS,
          options: {
            maxReconnectAttempts: -1,
            servers: [natsOptions.server],
            user: natsOptions.user,
            pass: natsOptions.pass,
            tls: {
              ca: [readFileSync(natsOptions.cert_path)],
              rejectUnauthorized: natsOptions.rejectUnauth,

            }
          }
        }
      }
    },
  ]),
  JwtModule.register({
  })
  ],
  controllers: [CacheController],
  providers: [CacheService, redisClientFactory, redisPubSubFactory, RedisRepository, RedisSubscriberService, RequestService]
})
export class CacheModule {
}


// import { Module } from '@nestjs/common';
// import { CacheController } from './cache.controller';
// import { CacheService } from './cache.service';
// import { ClientsModule, Transport } from '@nestjs/microservices';
// import { NatsRawSubscriber } from './natsRawSubscriber';
// import { readFileSync } from 'fs';
// import { ConfigModule } from '@nestjs/config';
// import { CacheServiceNatsProvider } from './cache.nats.provider';


// @Module({
//   imports: [ConfigModule.forRoot()],
//   controllers: [CacheController],
//   providers: [CacheService, NatsRawSubscriber]
// })
// export class CacheModule {
//   constructor() {
//     console.log("cache module init")
//   }
// }



// import { Module } from '@nestjs/common';
// import { CacheController } from './cache.controller';
// import { CacheService } from './cache.service';
// import { ClientsModule, Transport } from '@nestjs/microservices';
// import { NatsRawSubscriber } from '../old/natsRawSubscriber';
// import { readFileSync } from 'fs';
// import { ConfigModule, ConfigService } from '@nestjs/config';
// import configuration from 'src/config/configuration';
// import { RedisRepository } from './redis.repository';
// import { RedisSubscriberService } from './redis-subscriber.service';
// import { redisClientFactory, redisPubSubFactory } from './redis.clientFactory';
// import { NatsClientProvider } from './nats.client.provider';


// @Module({
//   imports: [ConfigModule.forRoot({
//     isGlobal: true,
//     load: [configuration]
//   })],
//   controllers: [CacheController],
//   providers: [CacheService, redisClientFactory, redisPubSubFactory, RedisRepository, RedisSubscriberService,NatsClientProvider]
// })
// export class CacheModule {
// }


// // import { Module } from '@nestjs/common';
// // import { CacheController } from './cache.controller';
// // import { CacheService } from './cache.service';
// // import { ClientsModule, Transport } from '@nestjs/microservices';
// // import { NatsRawSubscriber } from './natsRawSubscriber';
// // import { readFileSync } from 'fs';
// // import { ConfigModule } from '@nestjs/config';
// // import { CacheServiceNatsProvider } from './cache.nats.provider';


// // @Module({
// //   imports: [ConfigModule.forRoot()],
// //   controllers: [CacheController],
// //   providers: [CacheService, NatsRawSubscriber]
// // })
// // export class CacheModule {
// //   constructor() {
// //     console.log("cache module init")
// //   }
// // }
