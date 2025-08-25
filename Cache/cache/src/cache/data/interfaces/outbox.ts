export interface Outbox {
    Id: string;
    Event: string;
    OccuredAt: Date;
    Payload: string;
}
export interface StoreMailPayload {
    UserId: string;
    ConfirmationCode: string;
}
export interface ConfirmationMailPayload {
    UserId: string;
    ConfirmationCode: string;
    UserName: string;
    Name: string;
    DateOfBirtg: string
}