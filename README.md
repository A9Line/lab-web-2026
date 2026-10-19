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
