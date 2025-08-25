// import { MicroserviceOptions, Transport } from "@nestjs/microservices";
// import { readFileSync } from "fs";

// export function getNatsOptions(): MicroserviceOptions {
//     return {
//         transport: Transport.NATS,
//         options: {
//             servers: [process.env.NATS_SERVER ?? "localhost:4444"],
//             user: process.env.NATS_USER,
//             pass: process.env.NATS_PASS,
//             tls: {
//                 ca: [readFileSync(process.env.NATS_CA_CERT_PATH ?? "")],
//                 rejectUnauthorized: true,
//             }
//         }
//     }
// }