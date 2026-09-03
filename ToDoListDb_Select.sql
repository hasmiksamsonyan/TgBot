-- GetAllByUserId
SELECT
    "Id",
    "UserId",
    "ListId",
    "Name",
    "CreatedAt",
    "Deadline",
    "State",
    "StateChangedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111';


-- GetActiveByUserId
SELECT
    "Id",
    "UserId",
    "ListId",
    "Name",
    "CreatedAt",
    "Deadline",
    "State",
    "StateChangedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
  AND "State" = 0;


-- Get
SELECT
    "Id",
    "UserId",
    "ListId",
    "Name",
    "CreatedAt",
    "Deadline",
    "State",
    "StateChangedAt"
FROM "ToDoItem"
WHERE "Id" = 'aaaaaaaa-1111-1111-1111-111111111111';


-- ExistsByName
SELECT EXISTS
(
    SELECT 1
    FROM "ToDoItem"
    WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
      AND LOWER("Name") = LOWER('Подготовить отчёт')
);


-- CountActive
SELECT COUNT(*)
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
  AND "State" = 0;


-- Find
SELECT
    "Id",
    "UserId",
    "ListId",
    "Name",
    "CreatedAt",
    "Deadline",
    "State",
    "StateChangedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
  AND LOWER("Name") LIKE LOWER('Подготов%');