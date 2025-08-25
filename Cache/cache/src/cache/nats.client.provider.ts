import { Provider } from '@nestjs/common';
import { connect, NatsConnection, NatsError } from 'nats';
import { readFileSync } from 'fs';
import { ConfigService } from '@nestjs/config';

export const NATS_CONNECTION = 'NATS_CONNECTION';

export const NatsClientProvider: Provider = {
    provide: NATS_CONNECTION,
    useFactory: async (configService: ConfigService): Promise<NatsConnection> => {
        const natsOptions = configService.get('nats');

        const nc = await connect({
            servers: [natsOptions.server],
            user: natsOptions.user,
            pass: natsOptions.pass,
            tls: {
                ca: readFileSync(natsOptions.cert_path, 'utf8'),
            },
            reconnect: true,
            maxReconnectAttempts: -1,
            reconnectTimeWait: 2000, // 2 seconds between attempts
        });

        nc.closed()
            .then((err) => {
                if (err) {
                    console.error('NATS closed with error', err);
                } else {
                    console.log('NATS connection closed');
                }
            });

        // nc.status().then(async (statusAsync) => {
        //   for await (const s of statusAsync) {
        //     console.log(`[NATS Status] ${s.type}: ${s.data}`);
        //     if (s.type === 'disconnect') {
        //       // Custom reconnection logic or notification
        //       console.warn('NATS disconnected!');
        //     }
        //     if (s.type === 'reconnect') {
        //       console.info('NATS reconnected!');
        //     }
        //     if (s.type === 'error') {
        //       console.error('NATS error!', s.data);
        //     }
        //   }
        // });

        return nc;
    },
    inject: [ConfigService],
};
