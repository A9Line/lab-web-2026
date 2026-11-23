# Лабораторная работа 10. EF Core: сервер пишет в базу

Студент: Салават Исмагилов · Группа: ТРИС-2-23

Тема: Вывоз отходов лаборатории.

## Что сделано

Сервис заявок больше не хранит список в памяти, а работает с PostgreSQL через EF Core. Адреса, JSON и коды ответов остались как в ЛР8.

```text
backend/
├── Data/AppDbContext.cs
├── Migrations/
│   ├── 20261123165212_InitialCreate.cs
│   ├── 20261123165212_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Models/
│   ├── WasteRequest.cs
│   ├── Laboratory.cs
│   ├── WasteType.cs
│   └── User.cs
└── Services/WasteRequestService.cs
```

Пакеты: `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 и `Microsoft.EntityFrameworkCore.Design` 8.0.34 (ветка 8, как у проекта). Инструмент `dotnet-ef` 8.0.34 поставлен локально через `dotnet-tools.json`, чтобы у всех была одна версия: `dotnet tool restore`.

Сервис зарегистрирован как `Scoped`, потому что `AppDbContext` создаётся на каждый запрос. Контроллер почти не менялся: методы стали `async`.

## Строка подключения

Пароля в репозитории нет. Строка подключения лежит в user-secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=waste_removal;Username=postgres;Password=..."
```

Файл `secrets.json` хранится в профиле Windows (`%APPDATA%\Microsoft\UserSecrets\...`), а не в папке проекта. В `appsettings.json` ключ `ConnectionStrings:Default` пустой. Если строку не задать, сервер сразу пишет, где её задать, и не запускается.

Проверка `git grep -i password` находит только столбец `password_hash` и поле пароля на экране входа, самого пароля нет.

## Модель и таблицы

Имена в C# остались в PascalCase, а в базе такие же, как в `db/schema.sql` ЛР9. Всё настроено явно в `OnModelCreating`:

| C# | PostgreSQL |
|----|------------|
| `WasteRequest` | `waste_requests` |
| `Id` | `id` |
| `LaboratoryId` | `laboratory_id` |
| `WasteTypeId` | `waste_type_id` |
| `CreatedByUserId` | `created_by_user_id` |
| `AssigneeUserId` | `assignee_user_id` (NULL) |
| `CreatedAt` / `UpdatedAt` | `created_at` / `updated_at` |
| `Status` (enum) | `status` text, хранится строкой `New` / `InProgress` / ... |

Ограничения и индексы перенесены с теми же именами: CHECK на статус, длину `title` и `description`, класс опасности и роль; UNIQUE на `number`; FK `waste_requests_laboratory_id_fkey` и другие; индексы по `status`, `laboratory_id`, `assignee_user_id`.

Отличия от `schema.sql`: EF сам добавил индексы на два других внешних ключа (`IX_waste_requests_created_by_user_id`, `IX_waste_requests_waste_type_id`) и у `status` нет `DEFAULT 'New'` — статус всегда задаёт сервис.

Справочники (3 лаборатории, 4 типа отходов) и 4 учебных пользователя добавляются миграцией через `HasData`, данные те же, что в `seed.sql`. Без них POST некуда было бы привязать.

## Миграция

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Перед `database update` в учебной базе `waste_removal` удалил таблицы ЛР9, созданные `schema.sql`, чтобы схема была создана миграцией. Результат `\d waste_requests`:

```text
Индексы:
    "waste_requests_pkey" PRIMARY KEY, btree (id)
    "IX_waste_requests_created_by_user_id" btree (created_by_user_id)
    "IX_waste_requests_waste_type_id" btree (waste_type_id)
    "waste_requests_assignee_user_id_idx" btree (assignee_user_id)
    "waste_requests_laboratory_id_idx" btree (laboratory_id)
    "waste_requests_number_key" UNIQUE, btree (number)
    "waste_requests_status_idx" btree (status)
Ограничения-проверки:
    "waste_requests_description_check" CHECK (char_length(description) >= 10 AND char_length(description) <= 500)
    "waste_requests_status_check" CHECK (status = ANY (ARRAY['New'::text, 'InProgress'::text, 'Closed'::text, 'Cancelled'::text]))
    "waste_requests_title_check" CHECK (char_length(title) >= 5 AND char_length(title) <= 80)
Ограничения внешнего ключа:
    "waste_requests_assignee_user_id_fkey" FOREIGN KEY (assignee_user_id) REFERENCES users(id)
    "waste_requests_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users(id)
    "waste_requests_laboratory_id_fkey" FOREIGN KEY (laboratory_id) REFERENCES laboratories(id)
    "waste_requests_waste_type_id_fkey" FOREIGN KEY (waste_type_id) REFERENCES waste_types(id)
```

В `__EFMigrationsHistory` появилась строка `20261123165212_InitialCreate`.

## Что изменилось в API

- Номер `WR-2026-0001` считается от последнего номера за год в базе. Если два запроса одновременно получат один номер, база не даст вставить второй (UNIQUE), и сервер вернёт 409, а не 500.
- Если `laboratoryId`, `wasteTypeId`, `assigneeUserId` или `X-User-Id` указывают на запись, которой нет в базе, ответ 400 с ошибкой по этому полю. Без этой проверки внешний ключ привёл бы к 500.
- В коллекции Postman и `backend.http` пользователи заменены на существующих: 1 — заведующий, 2 и 3 — инженеры, тип отхода 1.

## Рестарт-тест

1. PostgreSQL запущен, API запущен на `http://localhost:5000`.
2. POST создал заявку:

```json
{"id":4,"number":"WR-2026-0004","title":"Вывоз ртутных ламп из 214","status":"New","laboratoryId":3,"wasteTypeId":3,"createdByUserId":1,"assigneeUserId":null, ...}
```

3. API остановлен и запущен снова.
4. `GET /api/waste-requests/4` → 200, те же поля:

```json
{"id":4,"number":"WR-2026-0004","title":"Вывоз ртутных ламп из 214","status":"New","laboratoryId":3,"wasteTypeId":3,"createdByUserId":1,"assigneeUserId":null, ...}
```

5. Та же строка в базе:

```text
 id |    number    |           title           | status
----+--------------+---------------------------+--------
  4 | WR-2026-0004 | Вывоз ртутных ламп из 214 | New
(1 строка)
```

## LINQ и SQL

В Development EF пишет в консоль выполненный SQL. Поиск заявки по id в сервисе:

```csharp
_db.WasteRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
```

EF выполнил:

```sql
SELECT w.id, w.assignee_user_id, w.created_at, w.created_by_user_id, w.description, w.laboratory_id, w.number, w.status, w.title, w.updated_at, w.waste_type_id
FROM waste_requests AS w
WHERE w.id = @__id_0
LIMIT 1
```

То есть `.Where`/`FirstOrDefault` с условием превращается в `WHERE`, а «первая или null» — в `LIMIT 1`. Если не найдено, сервис возвращает `null`, контроллер отвечает 404.

## Проверка

- [x] `dotnet build`: 0 ошибок, 0 предупреждений.
- [x] Миграция создана и применена, таблицы видны в `psql`.
- [x] Коллекция Postman прошла целиком (201, 400, 404, 409, оба PATCH).
- [x] Дополнительно 25 запросов: все коды как в ЛР8, плюс 400 на несуществующую лабораторию и пользователя.
- [x] Рестарт-тест: заявка id=4 на месте после перезапуска API.
- [x] Пароль и строка подключения не в Git, миграции в Git.

## Ответы на вопросы

Миграция отличается от `schema.sql` тем, что её создаёт EF по классам C#, и база помнит, какие миграции уже применены (`__EFMigrationsHistory`). Поменял модель — добавил новую миграцию, и `database update` применит только её. `schema.sql` просто пересоздаёт всё с нуля и про версии ничего не знает.

Пароль лежит в user-secrets в профиле Windows, вне репозитория. На другой машине его задают своей командой `dotnet user-secrets set` или переменной окружения `ConnectionStrings__Default`.

`FirstOrDefaultAsync(r => r.Id == id)` соответствует `SELECT ... FROM waste_requests WHERE id = ... LIMIT 1`.
