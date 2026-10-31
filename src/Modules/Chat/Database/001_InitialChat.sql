-- Apply once to the monolith's PostgreSQL database before enabling Chat.
BEGIN;
CREATE SCHEMA IF NOT EXISTS chat;
CREATE TABLE chat."Conversations" (
    "Id" uuid PRIMARY KEY,
    "UserA" uuid NOT NULL,
    "UserB" uuid NOT NULL,
    "Sequence" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);
CREATE UNIQUE INDEX "IX_Conversations_UserA_UserB" ON chat."Conversations" ("UserA", "UserB");
CREATE INDEX "IX_Conversations_UserB" ON chat."Conversations" ("UserB");
CREATE TABLE chat."Messages" (
    "Id" uuid PRIMARY KEY,
    "ConversationId" uuid NOT NULL REFERENCES chat."Conversations" ("Id") ON DELETE CASCADE,
    "SenderId" uuid NOT NULL,
    "ClientMessageId" uuid NOT NULL,
    "Sequence" bigint NOT NULL,
    "Body" character varying(4000) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);
CREATE UNIQUE INDEX "IX_Messages_ConversationId_Sequence" ON chat."Messages" ("ConversationId", "Sequence");
CREATE UNIQUE INDEX "IX_Messages_ConversationId_SenderId_ClientMessageId"
    ON chat."Messages" ("ConversationId", "SenderId", "ClientMessageId");
CREATE TABLE chat."Reads" (
    "ConversationId" uuid NOT NULL REFERENCES chat."Conversations" ("Id") ON DELETE CASCADE,
    "UserId" uuid NOT NULL,
    "Sequence" bigint NOT NULL,
    PRIMARY KEY ("ConversationId", "UserId")
);
CREATE TABLE chat."Outbox" (
    "Id" uuid PRIMARY KEY,
    "ConversationId" uuid NOT NULL,
    "Payload" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ProcessedAt" timestamp with time zone
);
CREATE INDEX "IX_Outbox_CreatedAt" ON chat."Outbox" ("CreatedAt") WHERE "ProcessedAt" IS NULL;
COMMIT;
