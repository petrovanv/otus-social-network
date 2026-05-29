# Social Network — Отус ДЗ1

Монолитное приложение: скелет социальной сети.

## Стек
- **Backend**: .NET 8 C#, ASP.NET Core Web API
- **Frontend**: Vue.js 3 + Vite
- **База данных**: MySQL 8

## Структура проекта
```
├── backend/SocialNetwork/   — ASP.NET Core API
├── frontend/social-network/ — Vue.js приложение
├── database/init.sql        — SQL-скрипт создания БД
└── postman/                 — Postman-коллекция
```

## API эндпоинты

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| POST | `/login` | Вход, получение JWT-токена | Нет |
| POST | `/user/register` | Регистрация пользователя | Нет |
| GET  | `/user/get/{id}` | Получение анкеты по ID | Bearer JWT |

---

## Локальный запуск

### 1. База данных (MySQL)

Выполните SQL-скрипт для создания БД и таблицы:

```bash
mysql -h 127.0.0.1 -u admin -p < database/init.sql
```

Или вручную в MySQL Workbench — открыть и выполнить `database/init.sql`.

### 2. Backend (.NET 8)

```bash
cd backend/SocialNetwork
dotnet run
```

API будет доступен на: **http://localhost:5000**  
Swagger UI: **http://localhost:5000/swagger**

#### Настройки подключения (appsettings.json)
Строка подключения уже настроена:
```json
"MySql": "Server=127.0.0.1;Port=3306;Database=social_network;Uid=admin;Pwd=Tenerife12!;"
```

### 3. Frontend (Vue.js)

```bash
cd frontend/social-network
npm install
npm run dev
```

Фронтенд будет доступен на: **http://localhost:5173**

---

## Тестирование через Postman

1. Импортируйте `postman/SocialNetwork.postman_collection.json` в Postman
2. Выполните запросы **по порядку**:
   - **1. Регистрация** → автоматически сохраняет `{{user_id}}`
   - **2. Вход** → автоматически сохраняет `{{token}}`
   - **3. Получить анкету** → использует оба сохранённых значения

---

## Пример работы с API

### Регистрация
```http
POST http://localhost:5000/user/register
Content-Type: application/json

{
  "first_name": "Иван",
  "second_name": "Иванов",
  "birthdate": "1990-05-15",
  "gender": "male",
  "biography": "Люблю программирование",
  "city": "Москва",
  "password": "secret123"
}
```
Ответ:
```json
{ "user_id": "550e8400-e29b-41d4-a716-446655440000" }
```

### Вход
```http
POST http://localhost:5000/login
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "password": "secret123"
}
```
Ответ:
```json
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
```

### Получение анкеты
```http
GET http://localhost:5000/user/get/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
Ответ:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "first_name": "Иван",
  "second_name": "Иванов",
  "birthdate": "1990-05-15",
  "gender": "male",
  "biography": "Люблю программирование",
  "city": "Москва"
}
```
