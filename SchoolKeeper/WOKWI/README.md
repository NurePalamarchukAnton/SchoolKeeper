# SchoolKeeper - WOKWI Random Events Generator

## Опис
Простий код для ESP32, який автоматично генерує та відправляє випадкові події безпеки на сервер через випадкові інтервали часу.

## Що робить код:

1. **Підключається до WiFi**
2. **Генерує випадкові події** кожні 5-30 секунд:
   - MotionSensor (датчик руху)
   - AlarmButton (кнопка тривоги)
   - AccessControl (контроль доступу)
3. **Відправляє події на сервер** через HTTP POST
4. **Виводить всю інформацію в Serial Monitor**

## Налаштування

### 1. WiFi налаштування
Відредагуйте в коді:
```cpp
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";
```

### 2. URL сервера
```cpp
const char* serverUrl = "http://your-server-url/api/Incident";
```

### 3. JWT токен
В функції `sendEventToServer()` замініть:
```cpp
http.addHeader("Authorization", "Bearer YOUR_JWT_TOKEN");
```

### 4. ID пристрою та школи
```cpp
const int DEVICE_ID = 1;
const int SCHOOL_ID = 1;
const int REPORTED_BY = 1; // ID користувача Security
```

## Використання в WOKWI

### Крок 1: Створіть проект
1. Відкрийте [WOKWI](https://wokwi.com)
2. Створіть новий проект з **ESP32 DevKit v1**

### Крок 2: Завантажте код
1. Скопіюйте код з `esp32_random_events.ino`
2. Вставте в редактор WOKWI
3. Встановіть бібліотеку **ArduinoJson** (через менеджер бібліотек)

### Крок 3: Налаштуйте параметри
Відредагуйте WiFi, URL сервера та JWT токен в коді

### Крок 4: Запустіть симуляцію
1. Натисніть "Start Simulation"
2. Відкрийте Serial Monitor (115200 baud)
3. Спостерігайте за автоматичною генерацією та відправкою подій

## Приклад виводу в Serial Monitor

```
========================================
SchoolKeeper Random Events Generator
========================================

Connecting to WiFi: YOUR_WIFI_SSID
........
✓ WiFi connected! IP address: 192.168.1.100

System ready!
First event in: 15 seconds

>>> GENERATING RANDOM EVENT <<<
Type: MotionSensor
Severity: High
Description: Рух виявлено в забороненій зоні

Sending to server: {"deviceId":1,"reportedBy":1,"incidentType":"MotionSensor","severity":"High","description":"Рух виявлено в забороненій зоні","timestamp":"2024-01-01T12:00:15","status":"Active","schoolId":1}

✓ Event sent successfully! Response code: 201
Response: {"statusCode":201,"data":{...},"message":"Incident created"}
----------------------------------------
Next event in: 23 seconds

>>> GENERATING RANDOM EVENT <<<
Type: AlarmButton
Severity: Critical
Description: Натиснуто кнопку тривоги!

Sending to server: {...}

✓ Event sent successfully! Response code: 201
----------------------------------------
Next event in: 8 seconds
```

## Типи подій та їх характеристики

### MotionSensor (Датчик руху)
- **Severity**: Medium або High (випадково)
- **Description**: Випадковий опис зі списку
- **Інтервал**: 5-30 секунд

### AlarmButton (Кнопка тривоги)
- **Severity**: Завжди Critical
- **Description**: "Натиснуто кнопку тривоги!"
- **Інтервал**: 5-30 секунд

### AccessControl (Контроль доступу)
- **Severity**: Завжди Low
- **Description**: "Спроба доступу з карткою: [випадковий UID]"
- **UID**: 8 випадкових hex символів
- **Інтервал**: 5-30 секунд

## Налаштування інтервалів

Щоб змінити інтервали між подіями, відредагуйте:
```cpp
nextEventDelay = random(5000, 30000); // Мінімум 5 сек, максимум 30 сек
```

Наприклад:
- `random(10000, 60000)` - кожні 10-60 секунд
- `random(2000, 10000)` - кожні 2-10 секунд

## Примітки

- У WOKWI симуляції WiFi може працювати не повністю
- Для тестування HTTP запитів використовуйте реальне обладнання
- Переконайтеся, що ваш сервер доступний з мережі ESP32
- JWT токен має бути валідним та не простроченим

## Troubleshooting

### WiFi не підключається
- Перевірте SSID та пароль
- У WOKWI WiFi може не працювати - використовуйте реальне обладнання

### HTTP запити не відправляються
- Перевірте URL сервера
- Перевірте JWT токен
- Перевірте, що сервер доступний
- Перевірте Serial Monitor для деталей помилок

### Події не генеруються
- Перевірте, що симуляція запущена
- Перевірте Serial Monitor
- Переконайтеся, що код завантажено правильно

