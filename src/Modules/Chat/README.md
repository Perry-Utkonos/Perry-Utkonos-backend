# Chat

Модуль личных текстовых переписок для монолита Perry.Api: ASP.NET Core 10, PostgreSQL и SignalR.

## Локальный запуск

Одинаковая команда для Windows, macOS и Linux. Из корня репозитория:

```sh
dotnet run --project tools/LocalLauncher
```

Лаунчер поднимает PostgreSQL в Docker и запускает API с профилем `ChatLocal`.

1. Откройте [страницу чата](http://localhost:7000/chat-local) в двух вкладках.
2. Войдите как `alice` в первой и `bob` во второй.
3. Откройте диалог и отправьте сообщение. Во второй вкладке выберите появившийся диалог и ответьте.

Вход демонстрационный, без паролей. Он доступен только локально в режиме Development. После перезапуска API войдите заново. Пользователь `charlie` нужен для проверки доступа: он не может читать диалог alice и bob.

## Остановка и база данных

API останавливается через `Ctrl+C` в терминале запуска. Из другого терминала:

```sh
dotnet run --project tools/LocalLauncher -- stop
```

Остановить PostgreSQL:

```sh
docker compose -f compose.chat-local.yml stop
```

Данные сохраняются в Docker volume. При первом запуске схема создаётся автоматически.

Для подключения к локальной базе: `127.0.0.1:55432`, база и пользователь — `chat_local`, пароль — `chat_local_only`.

Если запуск не удался, проверьте, что Docker запущен, а порт 7000 не занят другой копией API. Логи базы: `docker compose -f compose.chat-local.yml logs postgres`.

## Для разработки

- HTTP API описан в [Swagger](http://localhost:7000/swagger). Авторизованные запросы вне страницы чата требуют заголовок `Authorization: Bearer <token>`.
- SignalR: `/hubs/chat`, событие `MessageCreated`.
- Для повторной отправки используйте тот же `clientMessageId`: это предотвращает дубли.
- Проекты модуля: `Chat.Domain`, `Chat.Contracts`, `Chat.Application`, `Chat.Infrastructure`. HTTP API и SignalR находятся в `src/Perry.Api/Chat`.

Локальная страница показывает последние 50 сообщений. Вложения, редактирование, удаление и групповые чаты пока не реализованы. Текущая конфигурация рассчитана на один экземпляр API.

## Подключение к монолиту

Без профиля `ChatLocal` модуль включается через `Chat:Enabled`. Нужны `ConnectionStrings:Chat`, настройки JWT-провайдера `Chat:Authentication:Authority` и `Chat:Authentication:Audience`, а также реализация `IChatPolicy` для правил общения. Без неё создание диалога и новая отправка возвращают `503`.

Для общей базы схема создаётся скриптом `src/Modules/Chat/Database/001_InitialChat.sql`. Локальные демонстрационные пользователи не заменяют модуль регистрации и входа.
