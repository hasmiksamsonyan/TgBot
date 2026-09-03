CREATE TABLE "ToDoUser"
(
    "UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" TEXT NOT NULL,
    "RegisteredAt" TIMESTAMP NOT NULL
);

CREATE TABLE "ToDoList"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Name" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,

    CONSTRAINT "FK_ToDoList_ToDoUser"
        FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("UserId")
);

CREATE TABLE "ToDoItem"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "ListId" UUID NULL,
    "Name" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "Deadline" TIMESTAMP NOT NULL,
    "State" INTEGER NOT NULL,
    "StateChangedAt" TIMESTAMP NULL,

    CONSTRAINT "FK_ToDoItem_ToDoUser"
        FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("UserId"),

    CONSTRAINT "FK_ToDoItem_ToDoList"
        FOREIGN KEY ("ListId")
        REFERENCES "ToDoList" ("Id")
);

CREATE INDEX "IX_ToDoList_UserId"
    ON "ToDoList" ("UserId");

CREATE INDEX "IX_ToDoItem_UserId"
    ON "ToDoItem" ("UserId");

CREATE INDEX "IX_ToDoItem_ListId"
    ON "ToDoItem" ("ListId");

CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId"
    ON "ToDoUser" ("TelegramUserId");

    CREATE TABLE "Notification"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Type" TEXT NOT NULL,
    "Text" TEXT NOT NULL,
    "ScheduledAt" TIMESTAMP NOT NULL,
    "IsNotified" BOOLEAN NOT NULL,
    "NotifiedAt" TIMESTAMP NULL,

    CONSTRAINT "FK_Notification_ToDoUser"
        FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("UserId")
);

CREATE INDEX "IX_Notification_UserId"
    ON "Notification" ("UserId");
