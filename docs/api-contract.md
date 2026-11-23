# Контракт API

Базовый адрес в разработке: `http://localhost:5000`

Все тела запросов и ответов — в формате JSON.

## Таблица методов

| Метод и путь | Тело запроса | Успех | Возможная ошибка |
|--------------|--------------|-------|------------------|
| GET `/api/waste-requests` | — | 200, массив заявок или `[]` | — |
| GET `/api/waste-requests/{id}` | — | 200, объект заявки | 404 — заявки с таким id нет |
| POST `/api/waste-requests` | `title`, `laboratoryId`, `wasteTypeId`, `description` | 201, объект с `id`, `number`, `status: "New"` | 400 — невалидные поля |
| PATCH `/api/waste-requests/{id}/assignee` | `assigneeUserId` | 200, обновлённая заявка | 400 — невалидное тело; 404 — заявка не найдена; 409 — заявка уже не `New` |
| PATCH `/api/waste-requests/{id}/status` | `status` | 200, обновлённая заявка | 400 — неизвестный статус; 404 — заявка не найдена; 409 — недопустимый переход статуса |

**Клиент не присылает** при создании: `id`, `number`, `status`, `createdByUserId`, `assigneeUserId`, `createdAt`, `updatedAt`. Всё это определяет сервер.

**Текущий пользователь (с ЛР8 до ЛР11)** передаётся заголовком `X-User-Id: <id>` в POST и обоих PATCH. Без заголовка — 400. Из него сервер берёт `createdByUserId` и проверяет, что статус меняет назначенный исполнитель.

**Формат ошибок (с ЛР8).** Все отказы 400 / 404 / 409 / 500 возвращаются стандартным `ProblemDetails` ASP.NET Core (`application/problem+json`): поля `type`, `title`, `status`, `detail`, `traceId`. У 400 дополнительно словарь `errors` по полям. Примеры ниже приведены к этому виду.

**DELETE в API отсутствует.** Отмена — это `PATCH …/status` со значением `Cancelled`.

## Пример: создание заявки

**Запрос:**
```http
POST /api/waste-requests
Content-Type: application/json

{
  "title": "Вывоз отработанных кислот из лаборатории 214",
  "laboratoryId": 3,
  "wasteTypeId": 7,
  "description": "Накопилось 20 литров отработанной серной кислоты, требуется вывоз"
}
```

**Успех (201 Created):**
```json
{
  "id": 42,
  "number": "WR-2026-0042",
  "title": "Вывоз отработанных кислот из лаборатории 214",
  "description": "Накопилось 20 литров отработанной серной кислоты, требуется вывоз",
  "status": "New",
  "laboratoryId": 3,
  "wasteTypeId": 7,
  "createdByUserId": 5,
  "assigneeUserId": null,
  "createdAt": "2026-09-19T10:15:00Z",
  "updatedAt": "2026-09-19T10:15:00Z"
}
```

**Ошибка (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Заголовок должен содержать от 5 до 80 символов"],
    "description": ["Описание должно содержать от 10 до 500 символов"]
  },
  "traceId": "00-…"
}
```

## Пример: список заявок

**Запрос:**
```http
GET /api/waste-requests
```

**Успех (200 OK):**
```json
[
  {
    "id": 42,
    "number": "WR-2026-0042",
    "title": "Вывоз отработанных кислот из лаборатории 214",
    "laboratoryId": 3,
    "wasteTypeId": 7,
    "assigneeUserId": null,
    "status": "New"
  },
  {
    "id": 41,
    "number": "WR-2026-0041",
    "title": "Вывоз люминесцентных ламп",
    "laboratoryId": 1,
    "wasteTypeId": 4,
    "assigneeUserId": 9,
    "status": "InProgress"
  }
]
```

**Пустой список — это нормальный 200 и `[]`, а не 404.**

## Пример: назначение исполнителя

**Запрос:**
```http
PATCH /api/waste-requests/42/assignee
Content-Type: application/json

{
  "assigneeUserId": 9
}
```

**Успех (200 OK):**
```json
{
  "id": 42,
  "number": "WR-2026-0042",
  "status": "New",
  "assigneeUserId": 9,
  "updatedAt": "2026-09-19T11:00:00Z"
}
```

Статус остаётся `New` — назначение исполнителя не меняет статус.

## Пример: смена статуса

**Запрос (инженер берёт заявку в работу):**
```http
PATCH /api/waste-requests/42/status
Content-Type: application/json

{
  "status": "InProgress"
}
```

**Успех (200 OK):**
```json
{
  "id": 42,
  "number": "WR-2026-0042",
  "status": "InProgress",
  "assigneeUserId": 9,
  "updatedAt": "2026-09-19T11:30:00Z"
}
```

**Ошибка (409 Conflict)** — попытка сделать недопустимый переход, например `New → Closed` в один шаг или смена статуса чужой заявки:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Действие противоречит состоянию заявки",
  "status": 409,
  "detail": "Нельзя взять заявку в работу: исполнитель не назначен",
  "traceId": "00-…"
}
```

## Допустимые значения статуса

В API и в базе данных используются только четыре английских кода:

- `New` — на экране «Новая»
- `InProgress` — на экране «В работе»
- `Closed` — на экране «Выполнена»
- `Cancelled` — на экране «Отменена»