# Швидкий старт - WOKWI Simulator

## Швидкий запуск

1. **Запустіть сервер SchoolKeeper:**
   - Переконайтеся, що сервер працює на `http://localhost:8080`

2. **Запустіть симулятор (без токена):**
   ```bash
   dotnet run --project WokwiSimulator
   ```

3. **Або з повною конфігурацією:**
   ```bash
   dotnet run --project WokwiSimulator -- \
    --url http://localhost:8080/api/Incident/wokwi \
    --device-guid "550e8400-e29b-41d4-a716-446655440000" \
    --device-guid "123e4567-e89b-12d3-a456-426614174000"
   ```

**Примітка:** WOKWI ендпоинт (`/api/Incident/wokwi`) працює без авторизації, тому JWT токен не потрібен!

## Приклад використання

```bash
# Базовий запуск
dotnet run --project WokwiSimulator

# З іншими параметрами
dotnet run --project WokwiSimulator -- \
  --url http://localhost:8080/api/Incident/wokwi \
  --device-guids "550e8400-e29b-41d4-a716-446655440000,123e4567-e89b-12d3-a456-426614174000" \
  --min-delay 10 \
  --max-delay 60
```

## Зупинка

Натисніть `Ctrl+C` для зупинки симулятора.

## Вимоги

- .NET 9.0 SDK
- Запущений сервер SchoolKeeper
- DeviceGuid має відповідати існуючим девайсам, яким призначено школу (`SchoolId`)

**Примітка:** JWT токен не потрібен для WOKWI ендпоинту!

## Автозапуск через Docker Compose

Симулятор піднімається окремим сервісом `wokwi-simulator` разом із проєктом.

Налаштування робиться через `.env`:
- `WOKWI_DEFAULT_SCHOOL_ID` - (опційно) фіксований schoolId для автопризначення; якщо порожній, школа обирається випадково з БД
- `WOKWI_DEVICES_REFRESH_SECONDS` - як часто симулятор оновлює список девайсів з БД
- `WOKWI_DEVICE_GUIDS` - список GUID через кому
- `WOKWI_MIN_DELAY_SECONDS` - мінімальна затримка
- `WOKWI_MAX_DELAY_SECONDS` - максимальна затримка

Симулятор спочатку бере список девайсів з API `GET /api/Incident/wokwi/devices` (тільки активні девайси з призначеною школою) і випадково обирає GUID для відправки інциденту.
Якщо API тимчасово недоступний або список порожній, використовується `WOKWI_DEVICE_GUIDS` (fallback).

Після змін перезапустіть compose:
```bash
docker compose up -d --build
```

