import { Provider } from '@nestjs/common';
import { ClientProxyFactory, Transport } from '@nestjs/microservices';
import { readFileSync } from 'fs';

export const CacheServiceNatsProvider: Provider = {
    provide: 'cache_service',
    useFactory: () => {
        return ClientProxyFactory.create({
            transport: Transport.NATS,
            options: {
                servers: ['tls://localhost:4222'],
                user: 'cache_service',
                pass: 'anothersecret',
                tls: {
                    ca: [readFileSync("/home/aleksandar/Aleksandar/projects/mapmatch/NATS/certs/ca.crt")],
                    rejectUnauthorized: true,
                },
            },
        });
    },
};
