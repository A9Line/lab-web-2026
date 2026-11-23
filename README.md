# Информационная система управления вывозом лабораторных отходов

## Лабораторная работа

Лабораторные работы по дисциплине «РИС и Web».

## Тема проекта

Вывоз отходов лаборатории.

## Студент

ФИО: Салават Исмагилов

Группа: ТРИС-2-23

## Технологический стек

- Vue 3
- ASP.NET Core
- PostgreSQL

## Статус темы

На проверке

## Документация

- [Заявка на тему](docs/project-proposal.md)
- [Требования (ЛР1)](docs/lab-01-requirements.md)
- [Архитектура (ЛР2)](docs/architecture.md)
- [Модель данных (ЛР2)](docs/data-model.md)
- [Контракт API (ЛР2)](docs/api-contract.md)
- [Матрица требований (ЛР2)](docs/requirements-matrix.md)
- [Компоненты и маршруты (ЛР4)](docs/reports/lab-04.md)
- [Форма и список в памяти (ЛР5)](docs/reports/lab-05.md)
- [Клиент API и mock (ЛР6)](docs/reports/lab-06.md)
- [Каркас Web API (ЛР7)](docs/reports/lab-07.md)
- [REST: заявки и проверка полей (ЛР8)](docs/reports/lab-08.md)
- [Коллекция запросов Postman (ЛР8)](docs/api-collection/waste-requests.postman_collection.json)
- [PostgreSQL и SQL (ЛР9)](docs/reports/lab-09.md)

## Запуск клиента

Нужен Node.js LTS.

    cd frontend
    npm ci
    cp .env.example .env
    npm run dev

Открыть адрес Vite, обычно http://localhost:5173.

Маршруты клиента: `/` — список, `/new` — создание, `/login` — учебный экран входа.
Клиент работает с mock API: задержка 500 мс, данные живут в памяти вкладки и сбрасываются после F5.
`VITE_API_BASE_URL=http://localhost:5000` — адрес будущего API; `VITE_USE_MOCK=true` оставляет учебный mock. Настоящая интеграция будет в ЛР12.

Проверки клиента: `npm test` и `npm run build` из `frontend`.

## Запуск сервера

Нужен SDK .NET 8.0.400 или новее в ветке 8.0. В облачной среде установлен SDK 8.0.426 в `/workspace/.dotnet`.

```bash
cd backend
dotnet restore --locked-mode
dotnet run
```

API: `http://localhost:5000`.
Swagger в Development: `http://localhost:5000/swagger/index.html`.
Профиль `http` в `Properties/launchSettings.json` задаёт порт 5000 и среду Development.

```bash
curl http://localhost:5000/health
curl http://localhost:5000/info
```

`/health` возвращает `ok` и проверяет только живость процесса. `/info` возвращает название системы, среду и пометку ЛР8.
Успешный `/health` не доказывает создание заявки — для этого есть адреса заявок ниже.

### Заявки (ЛР8)

| Метод и путь | Успех | Отказы |
|---|---:|---|
| `GET /api/waste-requests` | 200, массив или `[]` | — |
| `GET /api/waste-requests/{id}` | 200 | 404 |
| `POST /api/waste-requests` | 201, `status: "New"` | 400 |
| `PATCH /api/waste-requests/{id}/assignee` | 200 | 400, 404, 409 |
| `PATCH /api/waste-requests/{id}/status` | 200 | 400, 404, 409 |

Текущий пользователь до ЛР11 передаётся заголовком `X-User-Id` в POST и обоих PATCH. Ошибки — стандартный `ProblemDetails`.

Коллекция запросов: Postman → Import → `docs/api-collection/waste-requests.postman_collection.json`, базовый URL `http://localhost:5000` (переменная `baseUrl`). Запускать по порядку на только что запущенном сервере. Те же запросы — в `backend/backend.http`.

Учебный сбой для проверки лога (только Development): `FailureSimulation__CreateWasteRequest=true dotnet run` — тогда валидный POST вернёт 500, а в консоли появится одна строка `CreateWasteRequest failed; …`. По умолчанию флаг выключен.

**Данные заявок пока в памяти сервера: после перезапуска `dotnet run` список пуст.** PostgreSQL будет подключён в следующих работах. Vue пока работает со своим mock и не подключается к backend (ЛР12).

Если в облачной среде команда `dotnet` не найдена, перед запуском выполните:

```bash
export DOTNET_ROOT=/workspace/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/workspace/.dotnet-home
export NUGET_PACKAGES=/workspace/.nuget/packages
```

Проверка сервера: `dotnet build --no-restore` из `backend`, затем `/health`, `/info`, Swagger и прогон коллекции запросов ЛР8.

## База данных (ЛР9)

Нужен PostgreSQL 17 (подойдёт и 16). Скрипты лежат в `db`, запускать из корня репозитория по порядку:

```powershell
createdb -U postgres waste_removal
psql -U postgres -d waste_removal -f db/schema.sql
psql -U postgres -d waste_removal -f db/seed.sql
psql -U postgres -d waste_removal -f db/queries.sql
```

В Windows сначала выполнить `$env:PGCLIENTENCODING="UTF8"`, иначе русские строки из файлов прочитаются в кодировке консоли.

`schema.sql` можно запускать повторно: он удаляет и создаёт заново только таблицы проекта. После него снова запустить `seed.sql`.

Пароль — тот, что задан при установке PostgreSQL, в репозиторий его не кладём. Backend к базе пока не подключён (ЛР10).
