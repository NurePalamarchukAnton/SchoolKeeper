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
#include <ArduinoJson.h>

// ========== Конфігурація WiFi ==========
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";
const char* serverUrl = "http://your-server-url/api/Incident";

// ========== Конфігурація пристрою ==========
const int DEVICE_ID = 1;
const int SCHOOL_ID = 1;
const int REPORTED_BY = 1; // ID користувача Security
String deviceLocation = "Головний вхід";

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
  Serial.println("SchoolKeeper Random Events Generator");
  Serial.println("========================================");
  Serial.println();
  
  // Підключення до WiFi
  connectToWiFi();
  
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
  // Перевірка з'єднання WiFi
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi disconnected, reconnecting...");
    connectToWiFi();
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
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("ERROR: WiFi not connected, cannot send event");
    return;
  }
  
  HTTPClient http;
  http.begin(serverUrl);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("Authorization", "Bearer YOUR_JWT_TOKEN"); // Замініть на ваш токен
  
  // Створення JSON
  StaticJsonDocument<512> doc;
  doc["deviceId"] = DEVICE_ID;
  doc["reportedBy"] = REPORTED_BY;
  doc["incidentType"] = eventType;
  doc["severity"] = severity;
  doc["description"] = description;
  doc["timestamp"] = getCurrentTimestamp();
  doc["status"] = "Active";
  doc["schoolId"] = SCHOOL_ID;
  
  String jsonString;
  serializeJson(doc, jsonString);
  
  Serial.print("Sending to server: ");
  Serial.println(jsonString);
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

String getCurrentTimestamp() {
  // Простий timestamp (для повноцінного рішення використовуйте NTP)
  unsigned long seconds = millis() / 1000;
  unsigned long hours = (seconds / 3600) % 24;
  unsigned long minutes = (seconds % 3600) / 60;
  seconds = seconds % 60;
  
  // Формат: 2024-01-01T12:00:00 (спрощений)
  String timestamp = "2024-01-01T";
  if (hours < 10) timestamp += "0";
  timestamp += String(hours) + ":";
  if (minutes < 10) timestamp += "0";
  timestamp += String(minutes) + ":";
  if (seconds < 10) timestamp += "0";
  timestamp += String(seconds);
  
  return timestamp;
}

void connectToWiFi() {
  Serial.print("Connecting to WiFi: ");
  Serial.println(ssid);
  
  WiFi.begin(ssid, password);
  
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 20) {
    delay(500);
    Serial.print(".");
    attempts++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println();
    Serial.print("✓ WiFi connected! IP address: ");
    Serial.println(WiFi.localIP());
    Serial.println();
  } else {
    Serial.println();
    Serial.println("✗ Failed to connect to WiFi");
    Serial.println("Events will not be sent until WiFi is connected");
    Serial.println();
  }
}

