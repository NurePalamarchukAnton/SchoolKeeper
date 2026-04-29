/*
 * SchoolKeeper - ESP32 Multi-Device Emulator
 * Емулює роботу 10 IoT пристроїв одночасно
 * 
 * Кожен пристрій має:
 * - Унікальний GUID (генерується при запуску)
 * - Тип пристрою (MotionSensor, AlarmButton, AccessControl)
 * - Незалежний таймер для генерації подій
 * 
 * Генерує випадкові події від різних пристроїв
 */

#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>

// ========== Конфігурація WiFi ==========
const char* ssid = "Wokwi-GUEST";
const char* password = "";
const int wifiChannel = 6;
// Локальний API SchoolKeeper для поточного проєкту.
// За потреби замініть host/port під ваш запуск.
const char* serverUrl = "http://localhost:8080/api/Incident/wokwi";

// WiFiClientSecure для HTTPS з'єднань
WiFiClientSecure client;

// Режим тестування
const bool TEST_MODE = true;

// ========== Конфігурація емуляції ==========
const int NUM_DEVICES = 10; // Кількість емульованих пристроїв

// Структура для зберігання інформації про пристрій
struct Device {
  String guid;
  String deviceType;      // MotionSensor, AlarmButton, AccessControl
  unsigned long lastEventTime;
  unsigned long nextEventDelay;
};

// Масив пристроїв
Device devices[NUM_DEVICES];

// ========== Типи пристроїв ==========
String deviceTypes[] = {
  "MotionSensor",
  "AlarmButton",
  "AccessControl"
};

// ========== Типи подій для кожного типу пристрою ==========
// MotionSensor може генерувати тільки MotionSensor події
// AlarmButton може генерувати тільки AlarmButton події
// AccessControl може генерувати тільки AccessControl події
// Але ми можемо рандомити який пристрій відправляє подію

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

String motionDescriptions[] = {
  "Підозріла активність виявлена",
  "Рух виявлено в забороненій зоні",
  "Несанкціонований рух у коридорі",
  "Активність після робочих годин",
  "Множинні рухи виявлено"
};

String alarmDescriptions[] = {
  "Натиснуто кнопку тривоги!",
  "Аварійна кнопка активована",
  "Ручна тривога",
  "Критична ситуація виявлена"
};

String accessDescriptions[] = {
  "Спроба доступу з карткою",
  "Несанкціонований доступ",
  "Доступ заборонено",
  "Картка не розпізнана",
  "Спроба доступу в заборонений час"
};

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("========================================");
  Serial.println("SchoolKeeper ESP32 Multi-Device Emulator");
  Serial.println("Емуляція " + String(NUM_DEVICES) + " пристроїв");
  Serial.println("========================================");
  Serial.println();
  
  if (TEST_MODE) {
    Serial.println("⚠️ TEST MODE ENABLED");
    Serial.println();
  }
  
  // Ініціалізація генератора випадкових чисел
  randomSeed(analogRead(0) + millis());
  
  // Генеруємо GUID та типи для всіх пристроїв
  Serial.println(">>> Ініціалізація пристроїв <<<");
  for (int i = 0; i < NUM_DEVICES; i++) {
    devices[i].guid = generateRandomGuid();
    devices[i].deviceType = deviceTypes[random(0, 3)]; // Випадковий тип пристрою
    devices[i].lastEventTime = millis();
    devices[i].nextEventDelay = random(5000, 30000); // Перша подія через 5-30 секунд
    
    Serial.print("Пристрій #");
    Serial.print(i + 1);
    Serial.print(": GUID=");
    Serial.print(devices[i].guid);
    Serial.print(", Type=");
    Serial.println(devices[i].deviceType);
  }
  Serial.println();
  
  Serial.print("Server URL: ");
  Serial.println(serverUrl);
  Serial.println();
  
  // Підключення до WiFi
  connectToWiFi();
  
  // Налаштування для HTTPS
  client.setInsecure();
  
  Serial.println("System ready!");
  Serial.println("Пристрої почнуть відправляти події незалежно один від одного");
  Serial.println();
}

void loop() {
  // Перевірка з'єднання WiFi
  static unsigned long lastWiFiCheck = 0;
  if (millis() - lastWiFiCheck > 10000) {
    if (WiFi.status() != WL_CONNECTED) {
      Serial.println("WiFi disconnected, reconnecting...");
      connectToWiFi();
    }
    lastWiFiCheck = millis();
  }
  
  // Перевіряємо кожен пристрій на предмет готовності відправити подію
  for (int i = 0; i < NUM_DEVICES; i++) {
    if (millis() - devices[i].lastEventTime >= devices[i].nextEventDelay) {
      // Генеруємо та відправляємо подію від цього пристрою
      generateAndSendEvent(i);
      
      // Генеруємо новий випадковий інтервал для цього пристрою (5-30 секунд)
      devices[i].nextEventDelay = random(5000, 30000);
      devices[i].lastEventTime = millis();
    }
  }
  
  delay(100); // Невелика затримка для стабільності
}

void generateAndSendEvent(int deviceIndex) {
  Device* device = &devices[deviceIndex];
  
  // Тип події завжди відповідає типу пристрою
  String eventType = device->deviceType;
  String severity;
  String description;
  
  // Генеруємо подію залежно від типу пристрою
  if (eventType == "MotionSensor") {
    severity = random(0, 2) == 0 ? "Medium" : "High";
    description = motionDescriptions[random(0, 5)];
  }
  else if (eventType == "AlarmButton") {
    severity = "Critical";
    description = alarmDescriptions[random(0, 4)];
  }
  else if (eventType == "AccessControl") {
    severity = "Low";
    String cardUID = generateRandomCardUID();
    description = accessDescriptions[random(0, 5)] + ": " + cardUID;
  }
  
  Serial.println(">>> ПОДІЯ ВІД ПРИСТРОЮ #" + String(deviceIndex + 1) + " <<<");
  Serial.print("GUID: ");
  Serial.println(device->guid);
  Serial.print("Device Type: ");
  Serial.println(device->deviceType);
  Serial.print("Event Type: ");
  Serial.println(eventType);
  Serial.print("Severity: ");
  Serial.println(severity);
  Serial.print("Description: ");
  Serial.println(description);
  Serial.println();
  
  // Відправка на сервер
  sendEventToServer(device->guid, eventType, severity, description);
}

void sendEventToServer(String deviceGuid, String eventType, String severity, String description) {
  // Створення JSON
  StaticJsonDocument<512> doc;
  doc["deviceGuid"] = deviceGuid;
  doc["incidentType"] = eventType;
  doc["severity"] = severity;
  doc["description"] = description;
  doc["status"] = "Active";
  
  String jsonString;
  serializeJson(doc, jsonString);
  
  Serial.print("JSON: ");
  Serial.println(jsonString);
  Serial.println();
  
  // Режим тестування
  if (TEST_MODE && WiFi.status() != WL_CONNECTED) {
    Serial.println("⚠️ TEST MODE: Attempting to send despite WiFi status");
    HTTPClient http;
    bool isHttps = String(serverUrl).startsWith("https://");
    
    if (isHttps) {
      http.begin(client, serverUrl);
    } else {
      http.begin(serverUrl);
    }
    
    http.addHeader("Content-Type", "application/json");
    http.setConnectTimeout(5000);
    http.setTimeout(10000);
    
    int httpResponseCode = http.POST(jsonString);
    
    if (httpResponseCode > 0) {
      Serial.print("✓ SUCCESS! Response: ");
      Serial.println(httpResponseCode);
      String response = http.getString();
      if (response.length() > 0) {
        Serial.print("Response body: ");
        Serial.println(response);
      }
    } else {
      Serial.print("✗ Failed. Code: ");
      Serial.println(httpResponseCode);
      Serial.print("Error: ");
      Serial.println(http.errorToString(httpResponseCode));
    }
    
    http.end();
    Serial.println("----------------------------------------");
    return;
  }
  
  // Реальна відправка
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("ERROR: WiFi not connected");
    return;
  }
  
  HTTPClient http;
  bool isHttps = String(serverUrl).startsWith("https://");
  
  if (isHttps) {
    http.begin(client, serverUrl);
  } else {
    http.begin(serverUrl);
  }
  
  http.addHeader("Content-Type", "application/json");
  http.setConnectTimeout(5000);
  http.setTimeout(10000);
  
  int httpResponseCode = http.POST(jsonString);
  
  if (httpResponseCode > 0) {
    Serial.print("✓ Event sent! Response: ");
    Serial.println(httpResponseCode);
    String response = http.getString();
    if (response.length() > 0) {
      Serial.print("Response: ");
      Serial.println(response);
    }
  } else {
    Serial.print("✗ Error. Code: ");
    Serial.println(httpResponseCode);
    Serial.print("Error: ");
    Serial.println(http.errorToString(httpResponseCode));
  }
  
  http.end();
  Serial.println("----------------------------------------");
}

String generateRandomGuid() {
  // Генеруємо випадковий GUID формату: ESP32_XXXXXXXXXXXX
  String guid = "ESP32_";
  char hexChars[] = "0123456789ABCDEF";
  
  for (int i = 0; i < 12; i++) {
    guid += hexChars[random(0, 16)];
  }
  
  return guid;
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

void connectToWiFi() {
  if (WiFi.status() == WL_CONNECTED) {
    return;
  }
  
  Serial.print("Connecting to WiFi: ");
  Serial.print(ssid);
  
  WiFi.begin(ssid, password, wifiChannel);
  
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 50) {
    delay(100);
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
    Serial.println();
    WiFi.disconnect();
    delay(1000);
  }
}
