import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { CacheModule } from './cache/cache.module';
import { ConfigModule } from '@nestjs/config';
import configuration from './config/configuration';

@Module({
  imports: [CacheModule, ConfigModule.forRoot({
    isGlobal: true,
    load: [configuration]
  })],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule { }
