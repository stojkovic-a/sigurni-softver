import { ConfirmationMailPayload } from "../interfaces/outbox";

export type EventType = "CONFIRMATION_MAIL";
export type EventPayloadMap = {
    CONFIRMATION_EMAIL: ConfirmationMailPayload;
}