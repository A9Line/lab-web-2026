-- ЛР9. Запросы. Запускать после schema.sql и seed.sql.

-- 1. Фильтр и сортировка: новые заявки, сначала свежие
SELECT number, title, status, created_at
FROM waste_requests
WHERE status = 'New'
ORDER BY created_at DESC;

-- 2. JOIN: заявки с названием лаборатории, типом отхода и исполнителем
-- LEFT JOIN для users, потому что у части заявок исполнителя ещё нет
SELECT r.number,
       r.title,
       l.name       AS laboratory_name,
       t.name       AS waste_type_name,
       u.full_name  AS assignee_name,
       r.status
FROM waste_requests AS r
JOIN laboratories AS l ON l.id = r.laboratory_id
JOIN waste_types AS t ON t.id = r.waste_type_id
LEFT JOIN users AS u ON u.id = r.assignee_user_id
ORDER BY r.created_at DESC;

-- 3. GROUP BY: сколько заявок в каждом статусе
SELECT status, COUNT(*) AS count
FROM waste_requests
GROUP BY status
ORDER BY status;

-- 4. GROUP BY для заведующего: заявки по лабораториям, сколько ещё не закрыто
SELECT l.name AS laboratory_name,
       COUNT(*) AS total,
       COUNT(*) FILTER (WHERE r.status IN ('New', 'InProgress')) AS open_count
FROM waste_requests AS r
JOIN laboratories AS l ON l.id = r.laboratory_id
GROUP BY l.name
ORDER BY open_count DESC, l.name;

-- 5. Проверка ограничений. Обе вставки должны упасть, данные не меняются.
-- CHECK: статуса Done нет среди четырёх допустимых
INSERT INTO waste_requests (number, title, description, status, laboratory_id, waste_type_id, created_by_user_id)
VALUES ('WR-2026-0099', 'Проверка статуса', 'Статус Done должен быть отклонён', 'Done', 1, 1, 1);

-- FK: лаборатории с id = 999 нет
INSERT INTO waste_requests (number, title, description, laboratory_id, waste_type_id, created_by_user_id)
VALUES ('WR-2026-0098', 'Проверка внешнего ключа', 'Лаборатории 999 не существует', 999, 1, 1);
