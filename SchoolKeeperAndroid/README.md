# SchoolKeeper Android

Клієнт на Kotlin з Jetpack Compose для API SchoolKeeper (`/api/...`).

## Підготовка

1. Встановіть [Android Studio](https://developer.android.com/studio) (Giraffe або новіше).
2. У корені `SchoolKeeperAndroid` створіть `local.properties` (або скопіюйте з `local.properties.example`):
   - `api.base.url=http://10.0.2.2:8080/` — емулятор Android звертається до `localhost` хоста ПК.
   - На фізичному пристрої замініть на `http://<LAN-IP>:8080/` (або адресу вашого load balancer).
3. Запустіть бекенд SchoolKeeper або load balancer на порту **8080**.

## Збірка

Відкрийте папку `SchoolKeeperAndroid` у Android Studio, дочекайтеся Gradle Sync і натисніть **Run** на емуляторі або пристрої.

Перший запуск завантажить Gradle Wrapper (якщо відсутній `gradle/wrapper/gradle-wrapper.jar`, Android Studio згенерує його при sync).

## Функціонал

- Вхід / реєстрація через ті самі ендпоінти, що й Razor (`POST /api/Auth/login`, `register`).
- Нижня навігація за ролями (як у веб-меню): головна, інциденти, звіти (де дозволяє API), пристрої (охорона), меню з ролевими екранами.
- Експорт звіту: JSON / CSV / TXT у внутрішнє сховище застосунку (`openFileOutput`).

## Примітки

- Для HTTP у debug використано `usesCleartextTraffic`; для продакшену краще HTTPS і вимкнути cleartext.
- Якщо зміните порт бекенда, оновіть `api.base.url` у `local.properties`.
