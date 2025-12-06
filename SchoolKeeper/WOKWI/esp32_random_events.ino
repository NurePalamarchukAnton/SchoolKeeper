/*
 * SchoolKeeper - ESP32 Random Events Generator
 * Простий код для WOKWI, який відправляє випадкові події на сервер
 * 
 * Генерує випадкові події:
 * - MotionSensor (датчик руху)
 * - AlarmButton (кнопка тривоги)
 * - AccessControl (контроль доступу)
 * 
 * Відправляє на сервер через випадкові інтервали часу
 */

#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>

// ========== Конфігурація WiFi ==========
// Для WOKWI симуляції (використовуйте ці налаштування):
//   ssid = "Wokwi-GUEST"
//   password = ""
//   channel = 6 (прискорює підключення в WOKWI)
// Для реального ESP32:
//   ssid = назва вашої WiFi мережі (наприклад, "MyHomeWiFi")
//   password = пароль від вашої WiFi мережі (наприклад, "mypassword123")
//   channel = 0 (автоматичний вибір каналу)
const char* ssid = "Wokwi-GUEST";        // Для WOKWI: "Wokwi-GUEST", для реального: ваша WiFi мережа
const char* password = "";                // Для WOKWI: "", для реального: пароль від WiFi
const int wifiChannel = 6;                // Для WOKWI: 6 (прискорює підключення), для реального: 0 (авто)

// URL сервера SchoolKeeper
// ⚠️ ВАЖЛИВО: Замініть "your-server-url" на реальний URL вашого сервера!
//
// Варіанти:
// 1. Для локального тестування (НЕ працює з WOKWI, тільки з реальним ESP32):
//    "http://192.168.1.100:5000/api/Incident/wokwi"  // IP вашого комп'ютера в локальній мережі
//
// 2. Для Production на Render:
//    "https://your-app-name.onrender.com/api/Incident/wokwi"
//
// 3. Для Production на Azure:
//    "https://your-app.azurewebsites.net/api/Incident/wokwi"
//
// 4. Для іншого хостингу:
//    "https://your-domain.com/api/Incident/wokwi"
//
// Примітка: В WOKWI симуляції localhost НЕ працює, потрібен публічний URL!
// Для HTTPS використовуйте https:// (код автоматично визначить протокол)
const char* serverUrl = "https://your-server-url/api/Incident/wokwi"; // ⚠️ ЗАМІНІТЬ НА РЕАЛЬНИЙ URL!

// WiFiClientSecure для HTTPS з'єднань
WiFiClientSecure client;

// Режим тестування (для WOKWI симуляції)
// Якщо true - показує що було б відправлено навіть без WiFi
// Якщо false - відправляє тільки при підключеному WiFi
const bool TEST_MODE = true; // Встановіть false для реального пристрою

// ========== Конфігурація пристрою ==========
// DeviceGuid буде автоматично згенерований з MAC-адреси ESP32
String deviceGuid = "";

// ========== Типи подій ==========
String eventTypes[] = {
  "MotionSensor",
  "AlarmButton",
  "AccessControl"
};

String severities[] = {
  "Low",
  "Medium",
  "High",
  "Critical"
};

String descriptions[] = {
  "Підозріла активність виявлена",
  "Натиснуто кнопку тривоги",
  "Спроба доступу з карткою: A1B2C3D4",
  "Рух виявлено в забороненій зоні",
  "Несанкціонований доступ",
  "Тривога активована вручну"
};

// ========== Таймери ==========
unsigned long lastEventTime = 0;
unsigned long nextEventDelay = 0;

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("========================================");
  Serial.println("SchoolKeeper ESP32 Device");
  Serial.println("WOKWI IoT Device for SchoolKeeper");
  Serial.println("========================================");
  Serial.println();
  
  if (TEST_MODE) {
    Serial.println("⚠ TEST MODE ENABLED");
    Serial.println("   Will attempt to send events even if WiFi status shows disconnected");
    Serial.println("   (May work in WOKWI simulation)");
    Serial.println();
  }
  
  // Генеруємо DeviceGuid з MAC-адреси ESP32
  generateDeviceGuid();
  Serial.print("Device GUID: ");
  Serial.println(deviceGuid);
  Serial.print("Server URL: ");
  Serial.println(serverUrl);
  Serial.println();
  
  // Підключення до WiFi
  connectToWiFi();
  
  // Налаштування для HTTPS (пропускаємо перевірку сертифіката для тестування)
  // ⚠️ Для production краще використовувати валідні сертифікати
  client.setInsecure();
  
  // Генеруємо перший випадковий інтервал (5-30 секунд)
  nextEventDelay = random(5000, 30000);
  lastEventTime = millis();
  
  Serial.println("System ready!");
  Serial.print("First event in: ");
  Serial.print(nextEventDelay / 1000);
  Serial.println(" seconds");
  Serial.println();
}

void loop() {
  // Перевірка з'єднання WiFi (перевіряємо рідше, щоб не спамити)
  static unsigned long lastWiFiCheck = 0;
  if (millis() - lastWiFiCheck > 10000) { // Перевіряємо кожні 10 секунд
    if (WiFi.status() != WL_CONNECTED) {
      Serial.println("WiFi disconnected, reconnecting...");
      connectToWiFi();
    }
    lastWiFiCheck = millis();
  }
  
  // Перевірка чи настав час для нової події
  if (millis() - lastEventTime >= nextEventDelay) {
    // Генеруємо та відправляємо випадкову подію
    generateAndSendRandomEvent();
    
    // Генеруємо новий випадковий інтервал (5-30 секунд)
    nextEventDelay = random(5000, 30000);
    lastEventTime = millis();
    
    Serial.print("Next event in: ");
    Serial.print(nextEventDelay / 1000);
    Serial.println(" seconds");
    Serial.println();
  }
  
  delay(1000); // Перевірка кожну секунду
}

void generateAndSendRandomEvent() {
  // Випадковий вибір типу події
  String eventType = eventTypes[random(0, 3)];
  
  // Випадковий вибір серйозності
  String severity = severities[random(0, 4)];
  
  // Випадковий вибір опису
  String description = descriptions[random(0, 6)];
  
  // Для AccessControl додаємо випадковий UID картки
  if (eventType == "AccessControl") {
    String cardUID = generateRandomCardUID();
    description = "Спроба доступу з карткою: " + cardUID;
    severity = "Low"; // AccessControl завжди Low
  }
  
  // Для AlarmButton завжди Critical
  if (eventType == "AlarmButton") {
    severity = "Critical";
    description = "Натиснуто кнопку тривоги!";
  }
  
  // Для MotionSensor зазвичай Medium або High
  if (eventType == "MotionSensor") {
    severity = random(0, 2) == 0 ? "Medium" : "High";
  }
  
  Serial.println(">>> GENERATING RANDOM EVENT <<<");
  Serial.print("Type: ");
  Serial.println(eventType);
  Serial.print("Severity: ");
  Serial.println(severity);
  Serial.print("Description: ");
  Serial.println(description);
  Serial.println();
  
  // Відправка на сервер
  sendEventToServer(eventType, severity, description);
}

void sendEventToServer(String eventType, String severity, String description) {
  // Створення JSON згідно з WokwiIncidentDto
  StaticJsonDocument<512> doc;
  doc["deviceGuid"] = deviceGuid;           // Обов'язкове поле - GUID пристрою
  doc["incidentType"] = eventType;           // Обов'язкове поле - тип інциденту
  doc["severity"] = severity;                // За замовчуванням "Low"
  doc["description"] = description;           // Опціональне поле
  doc["status"] = "Active";                  // За замовчуванням "Active"
  // timestamp опціональний - сервер встановить автоматично, якщо не вказано
  
  String jsonString;
  serializeJson(doc, jsonString);
  
  Serial.print("JSON payload: ");
  Serial.println(jsonString);
  Serial.println();
  
  // Режим тестування - намагаємося відправити навіть без "підключеного" WiFi
  // В WOKWI симуляції це може працювати
  if (TEST_MODE && WiFi.status() != WL_CONNECTED) {
    Serial.println("⚠ TEST MODE: Attempting to send despite WiFi status");
    Serial.println("⚠ Trying to send request (may work in WOKWI simulation):");
    Serial.print("   URL: ");
    Serial.println(serverUrl);
    Serial.print("   Method: POST");
    Serial.println();
    Serial.print("   Body: ");
    Serial.println(jsonString);
    Serial.println();
    
    // Пробуємо відправити навіть без "підключеного" WiFi
    HTTPClient http;
    
    // Визначаємо чи використовується HTTPS
    bool isHttps = String(serverUrl).startsWith("https://");
    
    if (isHttps) {
      // Для HTTPS використовуємо WiFiClientSecure
      http.begin(client, serverUrl);
    } else {
      // Для HTTP використовуємо звичайне підключення
      http.begin(serverUrl);
    }
    
    http.addHeader("Content-Type", "application/json");
    
    // Встановлюємо таймаути для HTTP запиту
    http.setConnectTimeout(5000);  // 5 секунд на підключення
    http.setTimeout(10000);        // 10 секунд на відповідь
    
    Serial.println("Attempting HTTP POST...");
    Serial.print("Protocol: ");
    Serial.println(isHttps ? "HTTPS" : "HTTP");
    Serial.print("WiFi status: ");
    Serial.println(WiFi.status());
    Serial.print("JSON payload length: ");
    Serial.println(jsonString.length());
    
    int httpResponseCode = http.POST(jsonString);
    
    if (httpResponseCode > 0) {
      Serial.print("✓ SUCCESS! Event sent! Response code: ");
      Serial.println(httpResponseCode);
      
      String response = http.getString();
      if (response.length() > 0) {
        Serial.print("Response: ");
        Serial.println(response);
      }
    } else {
      Serial.print("✗ Failed to send. Response code: ");
      Serial.println(httpResponseCode);
      Serial.print("Error: ");
      Serial.println(http.errorToString(httpResponseCode));
      Serial.println("(This is expected if WiFi is truly not available)");
    }
    
    http.end();
    Serial.println("----------------------------------------");
    return;
  }
  
  // Реальна відправка тільки якщо WiFi підключений (для реального пристрою)
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("ERROR: WiFi not connected, cannot send event");
    Serial.println("Set TEST_MODE = true to attempt sending anyway");
    return;
  }
  
  HTTPClient http;
  
  // Визначаємо чи використовується HTTPS
  bool isHttps = String(serverUrl).startsWith("https://");
  
  if (isHttps) {
    // Для HTTPS використовуємо WiFiClientSecure
    http.begin(client, serverUrl);
  } else {
    // Для HTTP використовуємо звичайне підключення
    http.begin(serverUrl);
  }
  
  http.addHeader("Content-Type", "application/json");
  // WOKWI ендпоинт не потребує авторизації - заголовок Authorization не потрібен
  
  // Встановлюємо таймаути для HTTP запиту
  http.setConnectTimeout(5000);  // 5 секунд на підключення
  http.setTimeout(10000);        // 10 секунд на відповідь
  
  Serial.print("Sending to server: ");
  Serial.println(serverUrl);
  Serial.print("Protocol: ");
  Serial.println(isHttps ? "HTTPS" : "HTTP");
  Serial.println();
  
  int httpResponseCode = http.POST(jsonString);
  
  if (httpResponseCode > 0) {
    Serial.print("✓ Event sent successfully! Response code: ");
    Serial.println(httpResponseCode);
    
    String response = http.getString();
    if (response.length() > 0) {
      Serial.print("Response: ");
      Serial.println(response);
    }
  } else {
    Serial.print("✗ Error sending event. Response code: ");
    Serial.println(httpResponseCode);
    Serial.print("Error: ");
    Serial.println(http.errorToString(httpResponseCode));
  }
  
  http.end();
  Serial.println("----------------------------------------");
}

String generateRandomCardUID() {
  // Генеруємо випадковий UID картки (8 hex символів)
  String uid = "";
  char hexChars[] = "0123456789ABCDEF";
  
  for (int i = 0; i < 8; i++) {
    uid += hexChars[random(0, 16)];
  }
  
  return uid;
}

void generateDeviceGuid() {
  // Генеруємо DeviceGuid з MAC-адреси ESP32
  // MAC-адреса має формат: XX:XX:XX:XX:XX:XX
  // Конвертуємо в формат GUID без двокрапок
  uint8_t mac[6];
  WiFi.macAddress(mac);
  
  deviceGuid = "";
  bool allZeros = true;
  
  // Перевіряємо чи MAC-адреса не всі нулі (може бути в WOKWI симуляції)
  for (int i = 0; i < 6; i++) {
    if (mac[i] != 0) {
      allZeros = false;
      break;
    }
  }
  
  if (allZeros) {
    // Якщо MAC-адреса недоступна (наприклад, в WOKWI симуляції),
    // генеруємо випадковий GUID на основі часу та випадкового числа
    randomSeed(analogRead(0) + millis());
    deviceGuid = "ESP32_";
    char hexChars[] = "0123456789ABCDEF";
    
    for (int i = 0; i < 12; i++) {
      deviceGuid += hexChars[random(0, 16)];
    }
    
    Serial.println("⚠ MAC address not available, using random GUID");
  } else {
    // Використовуємо реальний MAC-адрес
    char hexChars[] = "0123456789ABCDEF";
    
    for (int i = 0; i < 6; i++) {
      if (mac[i] < 16) deviceGuid += "0";
      String hexByte = String(mac[i], HEX);
      hexByte.toUpperCase();
      deviceGuid += hexByte;
    }
    
    // Додаємо префікс для кращої ідентифікації
    deviceGuid = "ESP32_" + deviceGuid;
  }
}

void connectToWiFi() {
  // Перевіряємо чи вже підключені
  if (WiFi.status() == WL_CONNECTED) {
    return;
  }
  
  Serial.print("Connecting to WiFi: ");
  Serial.print(ssid);
  
  // Використовуємо канал для прискорення підключення (особливо в WOKWI)
  WiFi.begin(ssid, password, wifiChannel);
  
  // Очікуємо підключення (як в прикладі WOKWI)
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 50) {
    delay(100);  // Менша затримка для швидшого підключення
    Serial.print(".");
    attempts++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println(" Connected!");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
    Serial.println();
  } else {
    Serial.println();
    Serial.println("✗ Failed to connect to WiFi");
    Serial.println("Note: In WOKWI, make sure you're using 'Wokwi-GUEST' as SSID");
    Serial.println("Events will not be sent until WiFi is connected");
    Serial.println();
    
    // Скидаємо стан для наступної спроби
    WiFi.disconnect();
    delay(1000);
  }
}

