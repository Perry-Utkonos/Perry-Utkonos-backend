# ChatApp — бэкенд чата 1-на-1 на ASP.NET Core

## Быстрый старт

### 1. Разложить файлы

Откройте PowerShell в папке с распакованными файлами и выполните:

```
powershell -ExecutionPolicy Bypass -File .\RESTRUCTURE.ps1
```

Скрипт создаст `src/ChatApp.Domain/`, `src/ChatApp.Application/`,
`src/ChatApp.Infrastructure/`, `src/ChatApp.Api/` и перенесёт файлы туда.

### 2. Проверить структуру

После скрипта должно быть:

```
.
├── ChatApp.sln
├── Directory.Build.props
├── docker-compose.yml
├── .env.example
├── README.md
└── src/
    ├── ChatApp.Domain/
    ├── ChatApp.Application/
    ├── ChatApp.Infrastructure/
    └── ChatApp.Api/
```

### 3. Запустить

```
cp .env.example .env
# отредактируйте .env — обязательно JWT_SIGNING_KEY
docker compose up --build
```

API поднимется на `http://localhost:8080`, Swagger — на `/swagger`.

## Архитектура

Классическая слоистая (Clean Architecture):

```
ChatApp.Domain          — сущности, enum'ы, доменные исключения
ChatApp.Application     — бизнес-логика, интерфейсы сервисов, DTO
ChatApp.Infrastructure  — EF Core DbContext, конфигурации, репозитории
ChatApp.Api             — Web API + SignalR Hub, аутентификация, DI
```

Зависимости направлены внутрь: `Api → Infrastructure → Application → Domain`.

## Стек

| Слой ↕▾ | Технология ↕▾ |
|---|---|
| −Runtime | .NET 8 |
| −Web | ASP.NET Core Web API |
| −Real-time | SignalR (+ Redis backplane) |
| −ORM | EF Core 8 + Npgsql |
| −БД | PostgreSQL 16 |
| −Аутентификация | JWT (access + refresh) |
| −Хеш паролей | BCrypt |
⚙

## Схема БД

- **users** — аккаунты
- **user_profile** — публичный профиль (1:1 к users, shared PK)
- **conversations** — диалоги 1-на-1
- **messages** — сообщения
- **message_reads** — позиция прочтения на пару (диалог, пользователь)
- **attachments** — метаданные вложений
- **refresh_tokens** — refresh-токены

## Миграции

```
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate \
    -p src/ChatApp.Infrastructure \
    -s src/ChatApp.Api \
    -o Persistence/Migrations

dotnet ef database update \
    -p src/ChatApp.Infrastructure \
    -s src/ChatApp.Api
```

При запуске в Development миграции применяются автоматически (`db.Database.MigrateAsync()`).

## Основные эндпоинты

### Auth

| Метод ↕▾ | Путь ↕▾ | Описание ↕▾ |
|---|---|---|
| −POST | `/api/auth/register` | Регистрация |
| −POST | `/api/auth/login` | Логин → access + refresh |
| −POST | `/api/auth/refresh` | Обновить access-токен |
⚙

### Conversations

| Метод ↕▾ | Путь ↕▾ | Описание ↕▾ |
|---|---|---|
| −GET | `/api/conversations` | Список диалогов пользователя |
| −POST | `/api/conversations` | Создать/получить диалог |
| −GET | `/api/conversations/{id}/messages` | История (курсорная пагинация) |
⚙

### SignalR Hub

`/hubs/chat` — WebSocket (JWT через `?access_token=`).

**Клиент → сервер:**

- `SendMessage(conversationId, clientMessageId, body)`
- `MarkRead(conversationId, lastReadSeq)`
- `Typing(conversationId)`

**Сервер → клиент:**

- `ReceiveMessage(MessageDto)`
- `ReadReceipt(conversationId, userId, lastReadSeq)`
- `UserTyping(conversationId, userId)`

### Пример JS-клиента

```
const conn = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:8080/hubs/chat", {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

conn.on("ReceiveMessage", (msg) => console.log("new:", msg));

await conn.start();
await conn.invoke("SendMessage", conversationId, crypto.randomUUID(), "Привет!");
```

## Ключевые принципы

1. **Сначала в БД, потом broadcast.** SignalR — транспорт, не хранилище.
2. **`clientMessageId` от клиента** — идемпотентность вставки.
3. **`seq` вместо `created_at`** — стабильная сортировка и cursor-пагинация.
4. **Hub тонкий, Service толстый.** Права проверяются на сервере.
5. **SignalR = свежесть, REST = полнота.** При реконнекте клиент догоняет через REST.

## Что не реализовано (сознательно)

- Загрузка файлов в S3/MinIO (в `Attachment` только метаданные)
- Push-уведомления (FCM/APNs)
- Presence («онлайн/оффлайн») с TTL в Redis
- Rate limiting на хаб
- Интеграционные тесты
- OpenTelemetry / метрики

Это скелет — каркас, на который легко навешиваются все эти фичи.
