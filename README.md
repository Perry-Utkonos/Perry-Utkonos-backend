# ChatApp — модульный монолит чата 1-на-1 на ASP.NET Core

Бэкенд для личного чата (1-на-1) на **ASP.NET Core 8** + **SignalR** + **EF Core 8** + **PostgreSQL**.
Один процесс — несколько логических модулей (Auth, Chat, Payment, Recommendations), связанных
через in-process Event Bus.

## Стек

|Слой ↕▾|Технология ↕▾|
|-|-|
|−Runtime|.NET 8|
|−Web|ASP.NET Core Web API|
|−Real-time|SignalR (Redis backplane — опционально)|
|ORM|EF Core 8 + Npgsql|
|БД|PostgreSQL 16|
|Хеш паролей|BCrypt|
|Swagger|Swashbuckle|
|⚙||

## Архитектура

Классическая слоистая структура с модульным разделением внутри Application/Domain:

```
ChatApp.Domain          — сущности, enum'ы, доменные исключения
ChatApp.Application     — бизнес-логика, интерфейсы, DTO, события
ChatApp.Infrastructure  — EF Core DbContext, конфигурации, репозитории
ChatApp.Api             — Web API + SignalR Hub, DI, middleware, обработчики событий
```

Зависимости: `Api → Infrastructure → Application → Domain`.

### Модули

|Модуль ↕▾|Что делает ↕▾|Схема БД ↕▾|
|-|-|-|
|−**Auth**|Создание пользователей|`public` (users, user\_profile)|
|**Chat**|Диалоги, сообщения, SignalR, read receipts|`public` (conversations, messages, message\_reads)|
|**Payment**|Создание и завершение платежей|`pay`|
|**Recommendations**|Рекомендации для пользователя|`rec`|
|⚙|||

Модули общаются **только через события** (`IEventBus`), прямых зависимостей между сервисами нет.

## Схема БД

|Таблица ↕▾|Схема ↕▾|Назначение ↕▾|
|-|-|-|
|−`users`|public|Аккаунты|
|`user\\\_profile`|public|Публичный профиль (1:1 с users, shared PK)|
|`refresh\\\_tokens`|public|Refresh-токены (задел под auth)|
|`conversations`|public|Диалоги 1-на-1|
|`messages`|public|Сообщения|
|`message\\\_reads`|public|Позиция прочтения|
|`attachments`|public|Метаданные вложений|
|`outbox\\\_messages`|public|Устаревшая (не используется)|
|`payments`|pay|Платежи|
|`recommendations`|rec|Рекомендации|
|⚙|||

## Требования

* .NET 8 SDK
* PostgreSQL 16 (локально или внешний сервер)
* Docker Desktop (для Redis, если хотите backplane)
* `dotnet-ef`: `dotnet tool install --global dotnet-ef`

## Быстрый старт

### 1\. Восстановить пакеты

```
dotnet restore
```

### 2\. Настроить connection string

Отредактируйте `src/ChatApp.Api/appsettings.json`:

```
{
  "ConnectionStrings": {
    "Postgres": "Host=127.0.0.1;Port=5432;Database=chatapp;Username=chatapp;Password=chatapp\\\_secret",
    "Redis": ""
  }
}
```

* `Postgres` — обязательно.
* `Redis` — опционально. Если пусто, SignalR работает без backplane (для одного инстанса этого достаточно).

### 3\. Создать и применить миграции

```
dotnet ef migrations add InitialCreate \\\\
    --project src/ChatApp.Infrastructure \\\\
    --startup-project src/ChatApp.Api \\\\
    --output-dir Persistence/Migrations

dotnet ef database update \\\\
    --project src/ChatApp.Infrastructure \\\\
    --startup-project src/ChatApp.Api
```

EF создаст таблицы `pay.payments` и `rec.recommendations` вместе с остальными (схемы `pay` и `rec` создаются автоматически).

### 4\. Запустить

**Visual Studio:** откройте `ChatApp.sln`, `F5`.
**CLI:**

```
dotnet run --project src/ChatApp.Api
```

API поднимется на `http://localhost:8080` (или на порту из `launchSettings.json`). Swagger — `/swagger`.

## Аутентификация (временная схема)

JWT пока нет. Идентификатор пользователя определяется через `ICurrentUserProvider`:

* **REST:** заголовок `X-User-Id: <guid>`
* **SignalR:** query-параметр `?userId=<guid>`

Когда появится auth-служба — заменяется одна регистрация в DI, код сервисов не меняется.

## API

### Auth

|Метод ↕▾|Путь ↕▾|Описание ↕▾|
|-|-|-|
|−POST|`/api/auth/register`|Создать пользователя (без токена)|
|⚙|||

### Conversations

|Метод ↕▾|Путь ↕▾|Описание ↕▾|
|-|-|-|
|−GET|`/api/conversations`|Список диалогов|
|−POST|`/api/conversations`|Создать/получить диалог|
|−GET|`/api/conversations/{id}/messages`|История (cursor-пагинация)|
|⚙|||

### Payments

|Метод ↕▾|Путь ↕▾|Описание ↕▾|
|-|-|-|
|−POST|`/api/payments`|Создать платёж|
|−GET|`/api/payments`|Список платежей пользователя|
|−GET|`/api/payments/{id}`|Платёж по id|
|−POST|`/api/payments/{id}/complete`|Пометить завершённым (в реале — вебхук)|
|⚙|||

### Recommendations

|Метод ↕▾|Путь ↕▾|Описание ↕▾|
|-|-|-|
|−GET|`/api/recommendations`|Список рекомендаций|
|−POST|`/api/recommendations/rebuild`|Пересчитать (заглушка)|
|⚙|||

### SignalR

`/hubs/chat` — WebSocket, идентификатор через `?userId=<guid>`.

**Клиент → сервер:** `SendMessage`, `MarkRead`, `Typing`
**Сервер → клиент:** `ReceiveMessage`, `ReadReceipt`, `UserTyping`

## Полный сценарий проверки

```
### 1. Регистрация alice
POST http://localhost:8080/api/auth/register
Content-Type: application/json

{ "username": "alice", "email": "alice@example.com", "password": "supersecret123" }

### 2. Регистрация bob
POST http://localhost:8080/api/auth/register
Content-Type: application/json

{ "username": "bob", "email": "bob@example.com", "password": "supersecret123" }

### 3. Создать диалог alice ↔ bob
POST http://localhost:8080/api/conversations
X-User-Id: <aliceId>
Content-Type: application/json

{ "peerId": "<bobId>" }

### 4. Отправить сообщение через SignalR (в браузере)

Откройте http://localhost:8080/swagger → DevTools → Console:

  const s = document.createElement('script');
  s.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js';
  document.head.appendChild(s);

  const conn = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:8080/hubs/chat?userId=<aliceId>")
    .withAutomaticReconnect().build();
  conn.on("ReceiveMessage", m => console.log("получено:", m));
  await conn.start();
  await conn.invoke("SendMessage", "<conversationId>", crypto.randomUUID(), "Привет!");

### 5. История
GET http://localhost:8080/api/conversations/<conversationId>/messages
X-User-Id: <aliceId>
```

## Структура

```
.
├── ChatApp.sln
├── Directory.Build.props
├── docker-compose.yml
├── .env.example
├── .gitignore
├── README.md
└── src/
    ├── ChatApp.Domain/
    │   ├── Entities/
    │   ├── Enums/
    │   └── Exceptions/
    ├── ChatApp.Application/
    │   ├── Abstractions/
    │   ├── Dtos/
    │   ├── Events/
    │   └── Services/
    ├── ChatApp.Infrastructure/
    │   ├── Persistence/
    │   │   └── Configurations/
    │   ├── Repositories/
    │   └── Services/
    └── ChatApp.Api/
        ├── Controllers/
        ├── Handlers/
        ├── Hubs/
        ├── Middleware/
        ├── Properties/
        ├── Services/
        ├── Program.cs
        ├── appsettings.json
        └── ChatApp.Api.http
```

