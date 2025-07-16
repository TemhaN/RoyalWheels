# 🚗 RoyalWheels API

**RoyalWheels** — API на **ASP.NET Core** для платформы аренды автомобилей.  
Позволяет искать, бронировать и арендовать автомобили, управлять профилем, отзывами, избранным, договорами лизинга и платежами.  
**Админ-панель** предоставляет полный контроль над пользователями, автомобилями и всеми данными.  
Использует **PostgreSQL**, **JWT-аутентификацию** и **Swagger** для документации.

## ✨ Возможности

- 🔐 **Аутентификация**: регистрация, вход, редактирование профиля через JWT
- 🚘 **Управление автомобилями**: поиск, фильтрация (по бренду, цене, году, статусу), сравнение
- 📝 **Отзывы**: добавление, просмотр, удаление отзывов (рейтинг 1–5)
- ⭐ **Избранное**: добавление/удаление автомобилей
- 🛒 **Бронирование и лизинг**: бронирование автомобилей, оформление договоров лизинга
- 💸 **Платежи**: управление платежами (создание, просмотр)
- 📊 **Аналитика**: статистика по лизингу, платежам, предпочтениям брендов, рекомендации
- 🛠️ **Админ-панель**: управление пользователями, автомобилями, договорами, платежами, отзывами
- 📚 **Swagger UI**: интерактивная документация API

## 📋 Требования

- [.NET SDK 6.0+](https://dotnet.microsoft.com/en-us/download)
- PostgreSQL (база данных настраивается через миграции)
- Современный браузер для Swagger UI (Chrome, Firefox, Edge)
- Подключение к серверу RoyalWheels (локально или удалённо)

## 🧩 Зависимости

| Библиотека / Технология                    | Назначение                                       |
|-------------------------------------------|--------------------------------------------------|
| `ASP.NET Core`                            | Основной фреймворк для API                       |
| `Entity Framework Core`                   | ORM для PostgreSQL                               |
| `Npgsql.EntityFrameworkCore.PostgreSQL`   | Поддержка PostgreSQL                             |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT-аутентификация                        |
| `BCrypt.Net-Next`                         | Хеширование паролей                              |
| `Swashbuckle.AspNetCore`                  | Документация API через Swagger                  |

Полный список — в `*.csproj` и `appsettings.json`.


## 🚀 Установка и запуск

1. **Клонируйте репозиторий**
   ```bash
   git clone https://github.com/yourusername/RoyalWheels.git
   cd RoyalWheels

2. **Установите зависимости**

   ```bash
   dotnet restore
   ```

3. **Настройте `appsettings.json`**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=LeasyDb;Username=postgres;Password=your_password;Include Error Detail=true;"
     },
     "Jwt": {
       "Key": "YourVeryLongSecretKey1234567890123456",
       "Issuer": "YourIssuer",
       "Audience": "YourAudience"
     }
   }
   ```

   ⚠️ Убедитесь, что `Jwt:Key` содержит не менее **32 символов**.

4. **Примените миграции EF Core**

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Запустите приложение**

   ```bash
   dotnet run
   ```

6. **Swagger UI**
   Откройте в браузере:
   [http://localhost:7247/swagger](http://localhost:7247/swagger)

7. **Релизная сборка**

   ```bash
   dotnet publish -c Release
   ```

   Сборка будет в `bin/Release/net6.0/publish`.
   

## 🖱️ Использование

* Запустите приложение: `dotnet run`
* Откройте Swagger UI:
  [http://localhost:7247/swagger](http://localhost:7247/swagger)

### 🔐 Аутентификация

**Регистрация**

```
POST /api/Auth/register
```

```json
{
  "FullName": "Имя",
  "Email": "email@example.com",
  "PhoneNumber": "+123456789",
  "Password": "password"
}
```

**Вход**

```
POST /api/Auth/login
```

```json
{
  "Email": "email@example.com",
  "Password": "password"
}
```

### 🚘 Работа с автомобилями

**Поиск**

```
GET /api/Cars/search?search=brand&minPrice=10000&maxYear=2023&status=Available
```

**Сравнение**

```
POST /api/Cars/compare
```

```json
{
  "CarIds": [1, 2, 3]
}
```

**Бронирование**

```
POST /api/Reservations
```

```json
{
  "CarId": 1,
  "ReservationStart": "2025-07-20T00:00:00",
  "ReservationEnd": "2025-07-25T00:00:00"
}
```

**Лизинг**

```
POST /api/Lease
```

```json
{
  "CarId": 1,
  "LeaseStartDate": "2025-07-20T00:00:00",
  "LeaseEndDate": "2026-07-20T00:00:00"
}
```

---

### ⭐ Избранное и отзывы

**Добавить в избранное**

```
POST /api/Cars/favorites/{carId}
```

**Добавить отзыв**

```
POST /api/Cars/{id}/review
```

```json
{
  "Rating": 5,
  "Comment": "Отличный автомобиль!"
}
```

### 📊 Аналитика

```
GET /api/Analytics
```

### 🛠️ Админ-панель (роль Admin)

* **Пользователи**: `/api/Admin/users`
* **Автомобили**: `/api/Admin/cars`
* **Договоры лизинга**: `/api/Admin/lease-contracts`
* **Платежи**: `/api/Admin/payments`
* **Отзывы**: `/api/Admin/reviews`

## 📦 Сборка приложения

**Релизная сборка**

```bash
dotnet publish -c Release
```

**Развёртывание на сервере**

1. Скопируйте содержимое `bin/Release/net6.0/publish` на сервер.
2. Настройте веб-сервер (Kestrel, Nginx, IIS).
3. Убедитесь, что PostgreSQL доступен и строка подключения актуальна.

## 📸 Скриншоты

<div style="display: flex; flex-wrap: wrap; gap: 10px; justify-content: center;">
  <img src="https://github.com/TemhaN/RoyalWheels/blob/master/Screenshots/1.png?raw=true" alt="Service App" width="30%">
  <img src="https://github.com/TemhaN/RoyalWheels/blob/master/Screenshots/2.png?raw=true" alt="Service App" width="30%">
  <img src="https://github.com/TemhaN/RoyalWheels/blob/master/Screenshots/3.png?raw=true" alt="Service App" width="30%">
  <img src="https://github.com/TemhaN/RoyalWheels/blob/master/Screenshots/4.png?raw=true" alt="Service App" width="30%">
</div>  

## 🧠 Автор

**TemhaN**  
[GitHub профиль](https://github.com/TemhaN)

## 🧾 Лицензия

Проект распространяется под лицензией [MIT License].

## 📬 Обратная связь

Нашли баг или хотите предложить улучшение?
Создайте **issue** или присылайте **pull request** в репозиторий!

## ⚙️ Технологии

* **ASP.NET Core** — Backend и маршрутизация
* **Entity Framework Core** — Работа с PostgreSQL
* **Npgsql** — Поддержка PostgreSQL
* **JWT** — Аутентификация
* **BCrypt** — Хеширование паролей
* **Swagger** — Документация API
* **CORS** — Кросс-доменные запросы
