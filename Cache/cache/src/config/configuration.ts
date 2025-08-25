export interface AppConfig {
    nats: {
        user: string;
        pass: string;
        server: string;
        cert_path: string;
        rejectUnauth: boolean;
    };
    auth: {
        secret: string;
        issuer: string;
        audience: string;   
        expires_minutes: number;
        ca_path: string;
    }
}

export default (): AppConfig => ({
    nats: {
        user: process.env.NATS_USER ?? "admin",
        pass: process.env.NATS_PASS ?? "admin",
        server: process.env.NATS_SERVER ?? "localhost:4222",
        cert_path: process.env.NATS_CA_CERT_PATH ?? "",
        rejectUnauth: true,
    },
    auth: {
        secret: process.env.JWT_SECRET ?? "",
        audience: process.env.JWT_AUDIENCE ?? "",
        issuer: process.env.JWT_ISSUER ?? "",
        expires_minutes: Number(process.env.JWT_EXPIRATION_MINUTES),
        ca_path: process.env.JWT_CA_PATH ?? ""
    }
});
