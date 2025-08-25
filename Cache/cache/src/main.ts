import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';
import { readFileSync } from 'fs';
import { ConfigService } from '@nestjs/config';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  const configService = app.get(ConfigService)
  const natsOptions = configService.get('nats')
  const microserviceNats = app.connectMicroservice<MicroserviceOptions>(
    {
      transport: Transport.NATS,
      options: {
        servers: [natsOptions.server],
        user: natsOptions.user,
        pass: natsOptions.pass,
        tls: {
          ca: [readFileSync(natsOptions.cert_path)],
          rejectUnauthorized: natsOptions.rejectUnauth,

        }
      }
    }
  );

  // const microserviceRedis = app.connectMicroservice<MicroserviceOptions>({
  //   transport: Transport.REDIS,
  //   options: {
  //     host: 'localhost',
  //     port: 6379
  //   }
  // })

  await app.startAllMicroservices();
  await app.init();
  // await app.listen(1111);
}
bootstrap();
