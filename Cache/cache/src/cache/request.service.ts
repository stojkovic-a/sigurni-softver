import { Injectable } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import { JwtService } from "@nestjs/jwt";
import axios from "axios";
import { Roles } from "./data/enums/roles";
import { readFileSync } from "fs";
import * as tls from "tls";

@Injectable()
export class RequestService {
    caCert: NonSharedBuffer;
    constructor(private readonly jwtService: JwtService, private readonly config: ConfigService) {
        const caCert = readFileSync(config.get("auth").ca_path)

    }
    async createSendRequest(data, targetUrl): Promise<number> {
        const payload = {
            roles: Roles[Roles.ADMIN]
        }
        const jwt_options = this.config.get("auth")
        const token = await this.jwtService.signAsync(payload, {
            secret: jwt_options.secret,
            algorithm: "HS256",
            issuer: jwt_options.issuer,
            audience: jwt_options.audience,
            expiresIn: jwt_options.expires_minutes
        })

        const response = await axios.post(
            targetUrl,
            data,
            {
                headers: {
                    Authorization: `Bearer ${token}`,
                    "Content-Type": `application/json`,
                },
                httpsAgent: new (require('https').Agent)({
                    rejectUnauthorized: true,
                    ca: this.caCert,
                    // checkServerIdentity: (host, cert) => {
                    // const expected = 'localhost';
                    // const err = tls.checkServerIdentity(expected, cert);
                    // if (err) {
                    // throw err;
                    // }
                    // }
                }),
            });
        return response.status;
    }
}