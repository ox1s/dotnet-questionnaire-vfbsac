# Полное описание проекта "Система Анкетирования"

## Общая информация

**Система Анкетирования (Anketa Project)** — это веб-приложение для проведения опросов и анкетирования среди различных групп пользователей (студенты, сотрудники, работодатели) с целью сбора и анализа данных об удовлетворенности образовательным процессом и другими аспектами деятельности учебного заведения.

Проект представляет собой полную переработку и модернизацию существующей системы на PHP. Новая система разрабатывается на стеке **ASP.NET Core** и **React** с применением принципов **Clean Architecture**.

---

## Технологический стек

### Backend
- **.NET 9** (ASP.NET Core)
- **PostgreSQL** (база данных)
- **Entity Framework Core** (Code-First подход)
- **MediatR** (реализация паттерна CQRS)
- **ErrorOr** (обработка ошибок без исключений)
- **JWT** (аутентификация)
- **OpenXML** (генерация DOCX отчетов)

### Frontend
- **React 18** с **TypeScript**
- **Material-UI (MUI)** (UI-библиотека)
- **React Router** (маршрутизация)
- **Recharts** (графики для отчетов)
- **Axios** (HTTP-клиент)
- **Zustand** (управление состоянием)
- **Vite** (сборщик)

### Архитектура
- **Clean Architecture** (4 слоя: Domain, Application, Infrastructure, Api)
- **CQRS** (разделение команд и запросов)
- **Repository Pattern** (через DbContext)
- **Unit of Work** (через SaveChangesAsync)

---

## Архитектура проекта

### Структура слоев

```
src/
├── Questionnaire.Domain/          # Доменный слой (сущности, бизнес-правила)
├── Questionnaire.Application/     # Слой приложения (Use Cases, CQRS)
├── Questionnaire.Infrastructure/  # Слой инфраструктуры (БД, внешние сервисы)
├── Questionnaire.Api/             # Слой представления (контроллеры, API)
└── Questionnaire.Contracts/       # DTO и контракты
```

### Принципы Clean Architecture

1. **Domain** — не зависит ни от чего, содержит только бизнес-логику
2. **Application** — зависит только от Domain, содержит сценарии использования
3. **Infrastructure** — зависит от Application, реализует внешние зависимости
4. **Api** — зависит от Application и Infrastructure, точка входа

---

## Реализованные компоненты

### 1. Доменные сущности (Domain Layer)

#### ✅ Реализовано:

- **User** — пользователь системы
  - `Id`, `Login`, `PasswordHash`
  - Связь с ролями через `UserRoles`

- **Role** — роли пользователей
  - `Id`, `Name` (admin, student, staff, hirer, departmentManager)
  - Связь с пользователями через `UserRoles`
  - Связь с анкетами через `FormRoles`

- **UserRole** — связь пользователя и роли (many-to-many)

- **Form** — анкета
  - `Id`, `Name`, `IsActive`
  - Связь с вопросами через `FormQuestions`
  - Связь с ролями через `FormRoles`

- **Question** — вопрос
  - `Id`, `Text`, `Type` (Rating, Text, Choice)
  - Связь с вариантами ответов через `QuestionOptions`

- **QuestionOption** — вариант ответа для вопросов типа Choice
  - `Id`, `Text`, `QuestionId`

- **QuestionType** — enum (Rating, Text, Choice)

- **FormQuestion** — связь анкеты и вопроса с порядком (many-to-many)
  - `FormId`, `QuestionId`, `Order`

- **FormRole** — связь анкеты и роли (many-to-many)
  - `FormId`, `RoleId`

- **Answer** — ответ пользователя на анкету
  - `Id`, `FormId`, `UserId`, `SubmittedDate`
  - `DisciplineId`, `TeacherId` (nullable, для будущей фильтрации)
  - Связь с деталями через `Details`

- **AnswerDetail** — деталь ответа на конкретный вопрос
  - `Id`, `AnswerId`, `QuestionId`
  - `Weight`, `Mark` (для Rating)
  - `TextResponse` (для Text)
  - Связь с выбранными опциями через `SelectedOptions`

- **AnswerDetailSelectedOption** — связь детали ответа и выбранного варианта (many-to-many)
  - `AnswerDetailId`, `QuestionOptionId`

#### ❌ Не реализовано (но упомянуто в README):

- **Department** — кафедра
- **Discipline** — дисциплина
- **Speciality** — специальность
- **Specialization** — специализация
- **Teacher** — преподаватель (как отдельная сущность)

**Примечание:** В `Answer` есть поля `DisciplineId` и `TeacherId`, но соответствующие таблицы и навигационные свойства отсутствуют.

---

### 2. Аутентификация и авторизация

#### ✅ Реализовано:

- **Регистрация** (`POST /api/auth/register`)
  - Создание пользователя с указанием роли
  - Хеширование пароля (BCrypt)
  - Генерация JWT токена

- **Вход** (`POST /api/auth/login`)
  - Проверка логина и пароля
  - Генерация JWT токена

- **JWT аутентификация**
  - Middleware для проверки токена
  - Атрибуты `[Authorize]` и `[Authorize(Roles = "admin")]`
  - Получение текущего пользователя через `ICurrentUserProvider`

#### ❌ Не реализовано:

- Обновление токена (refresh token)
- Восстановление пароля
- Смена пароля
- Управление пользователями через API (CRUD)

---

### 3. Управление вопросами (Admin)

#### ✅ Реализовано:

- **Создание вопроса** (`POST /api/admin/questions`)
  - Поддержка всех типов: Rating, Text, Choice
  - Для Choice — добавление вариантов ответов

- **Получение всех вопросов** (`GET /api/admin/questions`)
  - Возвращает список всех вопросов с вариантами

- **Удаление вопроса** (`DELETE /api/admin/questions/{id}`)

#### ❌ Не реализовано:

- Редактирование вопроса (`PUT /api/admin/questions/{id}`)
- Получение вопроса по ID
- Валидация при удалении (проверка использования в анкетах)

---

### 4. Управление анкетами (Admin)

#### ✅ Реализовано:

- **Создание анкеты** (`POST /api/forms`)
  - Создание с именем
  - По умолчанию `IsActive = true`

- **Получение всех анкет** (`GET /api/forms`)
  - Список всех анкет

- **Получение анкеты по ID** (`GET /api/forms/{id}`)
  - Возвращает анкету со всеми вопросами в порядке `Order`

- **Добавление вопроса в анкету** (`POST /api/forms/{formId}/questions/{questionId}`)
  - Указание порядка вопроса

- **Удаление вопроса из анкеты** (`DELETE /api/forms/{formId}/questions/{questionId}`)

- **Удаление анкеты** (`DELETE /api/forms/{id}`)

#### ❌ Не реализовано:

- Редактирование анкеты (изменение названия, `IsActive`)
- **Назначение ролей анкете** (нет API для управления `FormRoles`)
  - В БД есть таблица `FormRoles`, но нет эндпоинтов для её управления
  - В README упомянуто, что анкеты доступны по ролям, но UI/API для назначения отсутствует
- Переупорядочивание вопросов в анкете

---

### 5. Прохождение анкеты (User)

#### ✅ Реализовано:

- **Получение доступных анкет** (`GET /api/surveys`)
  - Фильтрация по ролям пользователя через `FormRoles`
  - Только активные анкеты (`IsActive = true`)

- **Отправка ответа** (`POST /api/surveys/submit`)
  - Валидация: оценка не может быть больше веса (для Rating)
  - Сохранение всех типов ответов:
    - Rating: `Weight` и `Mark`
    - Text: `TextResponse`
    - Choice: `SelectedOptions` (множественный выбор)

#### ❌ Не реализовано:

- Сбор контекстных данных при отправке:
  - `DisciplineId`, `TeacherId` (поля есть в `Answer`, но не собираются)
  - Другие метаданные (специальность, кафедра и т.д.)
- Проверка на повторное прохождение (можно ли проходить несколько раз?)
- Сохранение черновика ответа
- Просмотр истории своих ответов

---

### 6. Отчеты и аналитика (Admin)

#### ✅ Реализовано:

- **Сводный отчет** (`GET /api/reports/summary/{formId}`)
  - Агрегация данных по всем вопросам анкеты
  - Для Rating: средняя оценка, средний вес, количество ответов
  - Для Text: список всех текстовых ответов
  - Для Choice: количество выборов по каждому варианту

- **Экспорт отчета в DOCX** (`GET /api/reports/export/{formId}`)
  - Генерация Word-документа с данными отчета
  - Используется OpenXML

#### ❌ Не реализовано:

- **Сравнительный отчет** (`GET /api/reports/comparison`)
  - Упомянут в README как реализованный, но фактически отсутствует
  - Должен сравнивать данные за два периода

- **Фильтрация отчетов:**
  - По дате (период)
  - По кафедре (`DepartmentId`)
  - По дисциплине (`DisciplineId`)
  - По специальности (`SpecialityId`)
  - По преподавателю (`TeacherId`)
  - По другим параметрам

- Визуализация отчетов:
  - Графики для Rating вопросов (гистограммы, временные ряды)
  - Более детальная аналитика

---

### 7. Frontend (React)

#### ✅ Реализовано:

- **Аутентификация:**
  - Страница входа (`/login`)
  - Хранение токена в localStorage
  - Защищенные маршруты (`ProtectedRoute`)
  - Проверка ролей (`AdminRoute`)

- **Пользовательский интерфейс:**
  - Дашборд (`/`) — список доступных анкет
  - Страница прохождения анкеты (`/surveys/:id`)
    - Динамический рендеринг вопросов по типу
    - Валидация на клиенте (например, max для оценки = weight)

- **Административный интерфейс:**
  - Управление вопросами (`/admin/questions`)
    - Создание, просмотр, удаление
  - Управление анкетами (`/admin/forms`)
    - Создание, просмотр, удаление
    - Переход к редактированию
  - Редактирование анкеты (`/admin/forms/:id`)
    - Добавление/удаление вопросов
  - Просмотр отчета (`/admin/reports/:id`)
    - Отображение сводных данных
    - Графики для Choice вопросов (Recharts)

#### ❌ Не реализовано:

- **Управление ролями анкет:**
  - UI для назначения ролей анкете
  - Выбор ролей при создании/редактировании анкеты

- **Управление пользователями:**
  - Список пользователей
  - Создание/редактирование/удаление
  - Назначение ролей

- **Расширенная аналитика:**
  - Фильтры для отчетов (дата, кафедра, дисциплина и т.д.)
  - Сравнительные графики
  - Экспорт данных в других форматах (Excel, PDF)

- **Улучшения UX:**
  - Drag-and-drop для переупорядочивания вопросов
  - Предпросмотр анкеты
  - Редактирование вопросов и анкет
  - Активация/деактивация анкет через UI

---

## Недостающие компоненты (детально)

### 1. Справочные данные (Reference Data)

**Проблема:** В старой системе были кафедры, дисциплины, специальности, специализации, преподаватели. В новой системе эти сущности отсутствуют, хотя поля для них есть в `Answer`.

**Что нужно:**

```csharp
// Domain/Entities/Department.cs
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Discipline> Disciplines { get; set; }
    public ICollection<User> DepartmentManagers { get; set; } // для зав. кафедрой
}

// Domain/Entities/Discipline.cs
public class Discipline
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
}

// Domain/Entities/Speciality.cs
public class Speciality
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Specialization> Specializations { get; set; }
}

// Domain/Entities/Specialization.cs
public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SpecialityId { get; set; }
    public Speciality Speciality { get; set; }
}

// Domain/Entities/Teacher.cs
public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
```

**Действия:**
1. Создать миграции для новых таблиц
2. Добавить навигационные свойства в `Answer`
3. Создать CRUD API для справочников (опционально, можно заполнять вручную)
4. Обновить форму отправки анкеты для сбора этих данных

---

### 2. Управление ролями анкет (FormRoles)

**Проблема:** Таблица `FormRoles` существует в БД, но нет API и UI для её управления.

**Что нужно:**

```csharp
// Application/Forms/Commands/AssignRole/AssignRoleToFormCommand.cs
public record AssignRoleToFormCommand(int FormId, int RoleId) : IRequest<ErrorOr<Success>>;

// Application/Forms/Commands/RemoveRole/RemoveRoleFromFormCommand.cs
public record RemoveRoleFromFormCommand(int FormId, int RoleId) : IRequest<ErrorOr<Success>>;

// Application/Forms/Queries/GetFormRoles/GetFormRolesQuery.cs
public record GetFormRolesQuery(int FormId) : IRequest<ErrorOr<IEnumerable<Role>>>;
```

**API эндпоинты:**
- `POST /api/forms/{formId}/roles/{roleId}` — назначить роль
- `DELETE /api/forms/{formId}/roles/{roleId}` — убрать роль
- `GET /api/forms/{formId}/roles` — получить роли анкеты

**Frontend:**
- В `AdminFormDetailPage` добавить секцию "Доступные роли"
- Чекбоксы или мультиселект для выбора ролей
- Сохранение при редактировании анкеты

---

### 3. Сравнительный отчет (Comparison Report)

**Проблема:** В README указано как реализованное, но фактически отсутствует.

**Что нужно:**

```csharp
// Application/Reports/Queries/GetComparison/GetComparisonReportQuery.cs
public record GetComparisonReportQuery(
    int FormId,
    DateTime StartDate1,
    DateTime EndDate1,
    DateTime StartDate2,
    DateTime EndDate2
) : IRequest<ErrorOr<ComparisonReportResult>>;
```

**Логика:**
- Собрать данные за период 1
- Собрать данные за период 2
- Сравнить по каждому вопросу:
  - Для Rating: изменение средней оценки, изменение количества ответов
  - Для Choice: изменение распределения выборов
  - Для Text: сравнение частоты упоминаний тем (опционально, сложнее)

**API:**
- `GET /api/reports/comparison/{formId}?start1=...&end1=...&start2=...&end2=...`

**Frontend:**
- Страница сравнения с выбором периодов
- Графики сравнения (столбчатые, линейные)

---

### 4. Фильтрация отчетов

**Проблема:** Отчеты показывают все данные без возможности фильтрации.

**Что нужно:**

```csharp
// Application/Reports/Queries/GetSummary/GetSummaryReportQuery.cs
// Расширить запрос:
public record GetSummaryReportQuery(
    int FormId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? DepartmentId = null,
    int? DisciplineId = null,
    int? SpecialityId = null,
    int? TeacherId = null
) : IRequest<ErrorOr<SummaryReportResult>>;
```

**В обработчике:**
```csharp
var answers = await _context.Answers
    .Where(a => a.FormId == request.FormId)
    .Where(a => request.StartDate == null || a.SubmittedDate >= request.StartDate)
    .Where(a => request.EndDate == null || a.SubmittedDate <= request.EndDate)
    .Where(a => request.DepartmentId == null || a.DepartmentId == request.DepartmentId)
    // ... и т.д.
    .ToListAsync(cancellationToken);
```

**Frontend:**
- Форма фильтров на странице отчета
- Выпадающие списки для справочников
- Календарь для выбора периода

---

### 5. Управление пользователями (User Management)

**Проблема:** Можно только регистрировать, но нельзя управлять пользователями.

**Что нужно:**

```csharp
// Application/Users/Commands/Create/CreateUserCommand.cs
// Application/Users/Commands/Update/UpdateUserCommand.cs
// Application/Users/Commands/Delete/DeleteUserCommand.cs
// Application/Users/Commands/AssignRole/AssignRoleToUserCommand.cs
// Application/Users/Queries/GetAll/GetAllUsersQuery.cs
// Application/Users/Queries/GetById/GetUserByIdQuery.cs
```

**API:**
- `GET /api/admin/users` — список пользователей
- `GET /api/admin/users/{id}` — пользователь по ID
- `POST /api/admin/users` — создать пользователя
- `PUT /api/admin/users/{id}` — обновить пользователя
- `DELETE /api/admin/users/{id}` — удалить пользователя
- `POST /api/admin/users/{id}/roles/{roleId}` — назначить роль
- `DELETE /api/admin/users/{id}/roles/{roleId}` — убрать роль

**Frontend:**
- Страница `/admin/users`
- Таблица пользователей с ролями
- Модальное окно для создания/редактирования
- Управление ролями пользователя

---

### 6. Функциональность заведующего кафедрой (Department Manager)

**Проблема:** Роль `departmentManager` существует, но нет специальной функциональности.

**Что нужно:**

- Ограничение доступа к отчетам только по своей кафедре
- Фильтрация данных по кафедре в отчетах
- Возможно, отдельный дашборд для зав. кафедрой

**Реализация:**

```csharp
// Application/Common/Interfaces/ICurrentUserProvider.cs
// Добавить метод:
Task<Department?> GetUserDepartmentAsync();

// В ReportsController или отдельном DepartmentReportsController:
[Authorize(Roles = "admin,departmentManager")]
public async Task<IActionResult> GetDepartmentReport(...)
{
    var user = await _currentUserProvider.GetCurrentUserAsync();
    var department = await _currentUserProvider.GetUserDepartmentAsync();
    
    if (user.HasRole("departmentManager") && department != null)
    {
        // Фильтровать только по своей кафедре
    }
}
```

---

### 7. Редактирование сущностей

**Проблема:** Можно создавать и удалять, но нельзя редактировать.

**Что нужно:**

- `PUT /api/admin/questions/{id}` — редактирование вопроса
- `PUT /api/forms/{id}` — редактирование анкеты (название, IsActive)
- Frontend формы для редактирования

---

### 8. Дополнительные функции для анкет

**Что нужно:**

- Переупорядочивание вопросов (drag-and-drop или кнопки вверх/вниз)
- Копирование анкеты
- Предпросмотр анкеты перед публикацией
- История изменений анкеты (опционально)

---

### 9. Улучшения для прохождения анкеты

**Что нужно:**

- Проверка на повторное прохождение (настройка: можно ли проходить несколько раз?)
- Сохранение черновика (автосохранение каждые N секунд)
- Прогресс-бар прохождения
- Навигация между вопросами
- Валидация перед отправкой (проверка обязательных вопросов)

---

### 10. Расширенная аналитика

**Что нужно:**

- Временные графики (динамика по датам)
- Корреляционный анализ
- Экспорт в Excel (не только DOCX)
- Экспорт в PDF
- Детальные отчеты по отдельным ответам
- Статистика по пользователям (кто прошел, кто нет)

---

## Миграция данных из старой системы

В README упомянуты данные из старой PHP-системы. Для миграции нужно:

1. Создать скрипт миграции данных
2. Преобразовать денормализованные данные (JSON в полях) в нормализованные таблицы
3. Заполнить справочники (Departments, Disciplines и т.д.)
4. Создать пользователей и назначить роли
5. Создать анкеты и вопросы
6. Преобразовать ответы из старого формата в новый

---

## Резюме: Статус реализации

### ✅ Полностью реализовано:
- Базовая архитектура (Clean Architecture, CQRS)
- Аутентификация и авторизация (JWT)
- CRUD для вопросов (создание, получение, удаление)
- CRUD для анкет (создание, получение, удаление, добавление/удаление вопросов)
- Прохождение анкет (получение доступных, отправка ответов)
- Базовые отчеты (сводный отчет, экспорт в DOCX)
- Frontend: вход, дашборд, прохождение анкеты, админ-панель (частично)

### ⚠️ Частично реализовано:
- Отчеты (есть базовые, нет фильтрации и сравнения)
- Управление ролями анкет (таблица есть, API/UI нет)
- Контекстные данные в ответах (поля есть, сбор данных нет)

### ❌ Не реализовано:
- Справочные данные (Departments, Disciplines, Specialities, Specializations, Teachers)
- Сравнительные отчеты
- Фильтрация отчетов
- Управление пользователями
- Редактирование вопросов и анкет
- Функциональность заведующего кафедрой
- Расширенная аналитика
- Дополнительные функции (черновики, повторное прохождение и т.д.)

---

## Рекомендации по дальнейшей разработке

### Приоритет 1 (Критично):
1. Управление ролями анкет (FormRoles API/UI)
2. Редактирование вопросов и анкет
3. Справочные данные (хотя бы базовые сущности)

### Приоритет 2 (Важно):
4. Фильтрация отчетов
5. Сравнительные отчеты
6. Управление пользователями

### Приоритет 3 (Желательно):
7. Функциональность зав. кафедрой
8. Улучшения UX (drag-and-drop, предпросмотр)
9. Расширенная аналитика

---

## Заключение

Проект имеет **солидную архитектурную основу** и **базовый функционал**, но требует доработки для полноценного использования. Основные пробелы связаны с:
- Управлением ролями и пользователями
- Расширенной аналитикой и фильтрацией
- Справочными данными
- Редактированием существующих сущностей

Архитектура позволяет легко добавлять новые функции благодаря четкому разделению слоев и использованию CQRS.

