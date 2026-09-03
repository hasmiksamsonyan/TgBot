INSERT INTO "ToDoUser"
    ("UserId", "TelegramUserId", "TelegramUserName", "RegisteredAt")
VALUES
    (
        '11111111-1111-1111-1111-111111111111',
        100000001,
        'user_one',
        '2026-09-01 10:00:00'
    ),
    (
        '22222222-2222-2222-2222-222222222222',
        100000002,
        'user_two',
        '2026-09-01 11:00:00'
    );


INSERT INTO "ToDoList"
    ("Id", "UserId", "Name", "CreatedAt")
VALUES
    (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111111',
        'Работа',
        '2026-09-01 10:30:00'
    ),
    (
        'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
        '22222222-2222-2222-2222-222222222222',
        'Личное',
        '2026-09-01 11:30:00'
    );


INSERT INTO "ToDoItem"
    (
        "Id",
        "UserId",
        "ListId",
        "Name",
        "CreatedAt",
        "Deadline",
        "State",
        "StateChangedAt"
    )
VALUES
    (
        'aaaaaaaa-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        'Подготовить отчёт',
        '2026-09-01 10:40:00',
        '2026-09-05 18:00:00',
        0,
        NULL
    ),
    (
        'aaaaaaaa-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        'Позвонить клиенту',
        '2026-09-01 11:00:00',
        '2026-09-06 17:00:00',
        0,
        NULL
    ),
    (
        'bbbbbbbb-1111-1111-1111-111111111111',
        '22222222-2222-2222-2222-222222222222',
        NULL,
        'Купить продукты',
        '2026-09-01 11:40:00',
        '2026-09-04 19:00:00',
        1,
        '2026-09-02 12:00:00'
    ),
    (
        'bbbbbbbb-2222-2222-2222-222222222222',
        '22222222-2222-2222-2222-222222222222',
        'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
        'Записаться к врачу',
        '2026-09-01 12:00:00',
        '2026-09-07 16:00:00',
        0,
        NULL
    );