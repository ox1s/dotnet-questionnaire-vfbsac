# User Flow: Отчет Аналитики (UI → Backend)

## Обзор
Этот документ описывает полный поток данных от пользовательского интерфейса до backend для генерации отчета аналитики по анкетам.

---

## 1. UI → API Endpoint

### Пользователь инициирует запрос
Пользователь выбирает параметры для отчета аналитики:
- **Форма (анкета)**: ID формы для анализа
- **Срезы данных (Slices)**: Один или несколько временных периодов с фильтрами

### HTTP Request
```http
POST /reports/analytics
Authorization: Bearer {token}
Content-Type: application/json

{
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "slices": [
    {
      "label": "Весна 2026",
      "dateFrom": "2026-03-01T00:00:00Z",
      "dateTo": "2026-05-31T23:59:59Z",
      "disciplineId": "guid-optional",
      "teacherId": "guid-optional",
      "departmentId": "guid-optional",
      "specialityId": "guid-optional",
      "specializationId": "guid-optional",
      "organizationName": "optional-string"
    },
    {
      "label": "Осень 2025",
      "dateFrom": "2025-09-01T00:00:00Z",
      "dateTo": "2025-11-30T23:59:59Z"
    }
  ]
}
```

**Альтернативный endpoint для скачивания Word документа:**
```http
POST /reports/word
```
(Принимает те же параметры, возвращает .docx файл)

---

## 2. API Layer → Application Layer

### Endpoint: `GetAnalytics.cs`
**Расположение:** `src/Web.Api/Endpoints/Reports/GetAnalytics.cs`

```csharp
app.MapPost("reports/analytics", async (
    GetAnalyticsReportQuery query,
    IQueryHandler<GetAnalyticsReportQuery, AnalyticsReportResponse> handler,
    CancellationToken cancellationToken) =>
{
    Result<AnalyticsReportResponse> result = await handler.Handle(query, cancellationToken);
    return result.Match(Results.Ok, CustomResults.Problem);
})
.WithTags("Reports")
.RequireAuthorization();
```

**Что происходит:**
- ASP.NET автоматически десериализует JSON в `GetAnalyticsReportQuery`
- Endpoint требует авторизации (JWT token)
- Делегирует обработку в Application Layer через CQRS паттерн

---

## 3. Application Layer: Query Handler

### `GetAnalyticsReportQueryHandler`
**Расположение:** `src/Application/Reports/Queries/GetAnalytics/GetAnalyticsReportQueryHandler.cs`

```csharp
public async Task<Result<AnalyticsReportResponse>> Handle(
    GetAnalyticsReportQuery query,
    CancellationToken cancellationToken)
{
    // Валидация: минимум 1 срез обязателен
    if (query.Slices.Count == 0)
    {
        return Result.Failure<AnalyticsReportResponse>(
            Error.Validation("Analytics.SlicesRequired", 
                "At least one analytics slice is required."));
    }

    // Делегирование построения отчета
    return await analyticsReportBuilder.BuildAsync(
        query.FormId,
        query.Slices,
        cancellationToken);
}
```

**Ответственность:**
- Базовая валидация входных данных
- Делегирование бизнес-логики в `IAnalyticsReportBuilder`

---

## 4. Report Builder: Оркестрация процесса

### `AnalyticsReportBuilder`
**Расположение:** `src/Application/Reports/Queries/GetAnalytics/AnalyticsReportBuilder.cs`

### Шаг 4.1: Загрузка формы
```csharp
FormProjection? form = await LoadFormAsync(formId, cancellationToken);
```

**Что загружается:**
- ID и название формы
- Только числовые вопросы (Number, Rating, WeightedRating)
- Вопросы отсортированы по порядку (Order)

**Проекция:**
```csharp
FormProjection(
    Id: Guid,
    Title: string,
    Questions: List<QuestionProjection>
)

QuestionProjection(
    Id: Guid,
    Text: string,
    Type: QuestionType,
    Order: int
)
```

### Шаг 4.2: Обработка каждого среза
Для каждого `AnalyticsSliceRequest` в запросе:

```csharp
foreach (AnalyticsSliceRequest slice in slices)
{
    AnalyticsSliceResult sliceResult = await BuildSliceAsync(form, slice, cancellationToken);
    sliceResults.Add(sliceResult);
}
```

---

## 5. Построение среза данных

### `BuildSliceAsync` - детальный процесс

#### 5.1 Построение запроса к БД
**Компонент:** `SubmissionQueryBuilder`

```csharp
IQueryable<Submission> submissionsQuery = queryBuilder.BuildQuery(
    form.Id,
    slice.DateFrom,
    slice.DateTo,
    filters);
```

**Применяемые фильтры:**
- ✅ FormId (обязательно)
- ✅ Временной диапазон (dateFrom - dateTo, нормализованный до полных дней)
- ✅ DisciplineId (опционально)
- ✅ TeacherId (опционально)
- ✅ DepartmentId (опционально)
- ✅ SpecialityId (опционально)
- ✅ SpecializationId (опционально)
- ✅ OrganizationName (опционально, LIKE поиск)

**Нормализация дат:**
```csharp
DateTime normalizedFrom = dateFrom.Date;  // 00:00:00
DateTime normalizedToExclusive = dateTo.Date.AddDays(1);  // следующий день 00:00:00
```

#### 5.2 Подсчет общего количества ответов
```csharp
int totalSubmissions = await submissionsQuery.CountAsync(cancellationToken);
```

#### 5.3 Агрегация данных по вопросам
**Компонент:** `QuestionAggregator`

```csharp
List<QuestionAggregateProjection> aggregates = await aggregator.AggregateAsync(
    submissionsQuery,
    cancellationToken);
```

**SQL логика (упрощенно):**
```sql
SELECT 
    QuestionId,
    AVG(NumericValue) as RawAverage,
    AVG(NumericValue * NumericValue) as RawAverageSquares,
    SUM(CASE WHEN Weight > 0 THEN NumericValue / Weight * 10 ELSE 0 END) as WeightedNormalizedSum,
    SUM(CASE WHEN Weight > 0 THEN 1 ELSE 0 END) as WeightedCount,
    COUNT(*) as SubmissionCount
FROM Answers
WHERE NumericValue IS NOT NULL
GROUP BY QuestionId
```

**Результат:**
```csharp
QuestionAggregateProjection(
    QuestionId: Guid,
    RawAverage: decimal,           // Среднее значение ответов
    RawAverageSquares: decimal,    // Среднее квадратов (для расчета σ)
    WeightedNormalizedSum: decimal, // Сумма взвешенных нормализованных значений
    WeightedCount: int,            // Количество взвешенных ответов
    SubmissionCount: int           // Общее количество ответов
)
```

#### 5.4 Расчет метрик для каждого вопроса
**Компонент:** `MetricCalculator`

```csharp
foreach (QuestionProjection question in form.Questions)
{
    aggregatesByQuestionId.TryGetValue(question.Id, out QuestionAggregateProjection? aggregate);
    SliceQuestionMetric metric = calculator.Calculate(question.Type, aggregate);
    metricsByQuestionId[question.Id] = metric;
}
```

**Расчет ResultScore:**
- **WeightedRating:** `WeightedNormalizedSum / WeightedCount`
- **Number/Rating:** `RawAverage`

**Расчет StandardDeviation (σ):**
```csharp
variance = RawAverageSquares - (RawAverage * RawAverage)
standardDeviation = √variance
```

**Результат:**
```csharp
SliceQuestionMetric(
    AverageScore: decimal,        // Среднее значение
    ResultScore: decimal,         // Итоговый балл (с учетом весов)
    StandardDeviation: decimal,   // Стандартное отклонение
    SubmissionCount: int          // Количество ответов
)
```

#### 5.5 Расчет общих метрик среза
```csharp
(decimal overallAverage, decimal overallStdDev) = calculator.CalculateOverallMetrics(
    metricsByQuestionId.Values);
```

**Формулы:**
- **OverallAverage:** Среднее всех ResultScore
- **OverallStdDev:** RMS (Root Mean Square) всех StandardDeviation
  ```
  σ_pooled = √(Σ(σᵢ²) / n)
  ```

---

## 6. Маппинг в Response

### `ResponseMapper`
**Расположение:** `src/Application/Reports/Queries/GetAnalytics/ResponseMapper.cs`

Преобразует внутренние проекции в DTO для API:

```csharp
AnalyticsReportResponse {
    FormId: Guid,
    FormTitle: string,
    Slices: [
        {
            Label: "Весна 2026",
            DateFrom: DateTime,
            DateTo: DateTime,
            TotalSubmissions: int,
            OverallAverage: decimal,
            OverallStandardDeviation: decimal,
            Filters: AnalyticsFilterSet
        }
    ],
    Questions: [
        {
            QuestionId: Guid,
            QuestionText: string,
            QuestionType: "Rating",
            Order: int,
            SliceMetrics: [
                {
                    SliceLabel: "Весна 2026",
                    AverageScore: decimal,
                    ResultScore: decimal,
                    StandardDeviation: decimal,
                    SubmissionCount: int
                },
                {
                    SliceLabel: "Осень 2025",
                    ...
                }
            ]
        }
    ]
}
```

**Структура данных:**
- **Slices:** Метаданные и общие метрики для каждого среза
- **Questions:** Каждый вопрос содержит метрики по всем срезам (для сравнения)

---

## 7. Response → UI

### HTTP Response
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "formTitle": "Оценка качества преподавания",
  "slices": [
    {
      "label": "Весна 2026",
      "dateFrom": "2026-03-01T00:00:00Z",
      "dateTo": "2026-05-31T23:59:59.9999999Z",
      "totalSubmissions": 150,
      "overallAverage": 8.5,
      "overallStandardDeviation": 1.2,
      "filters": {
        "disciplineId": "guid",
        "teacherId": null,
        ...
      }
    }
  ],
  "questions": [
    {
      "questionId": "guid",
      "questionText": "Оцените качество материала",
      "questionType": "Rating",
      "order": 1,
      "sliceMetrics": [
        {
          "sliceLabel": "Весна 2026",
          "averageScore": 8.7,
          "resultScore": 8.7,
          "standardDeviation": 1.1,
          "submissionCount": 150
        }
      ]
    }
  ]
}
```

### UI отображает:
- 📊 Сравнительные графики по срезам
- 📈 Метрики для каждого вопроса
- 📉 Стандартные отклонения (разброс ответов)
- 🔢 Количество ответов в каждом срезе

---

## 8. Альтернативный Flow: Скачивание Word документа

### Endpoint: `/reports/word`

```csharp
Result<AnalyticsReportResponse> result = await handler.Handle(query, cancellationToken);

if (result.IsFailure)
{
    return CustomResults.Problem(result);
}

byte[] fileBytes = await reportGenerator.GenerateAnalyticsReport(
    result.Value, 
    cancellationToken);

return Results.File(
    fileBytes,
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    $"report_{query.FormId}.docx");
```

**Процесс:**
1. Те же шаги 1-6 (получение данных)
2. `IReportGenerator` преобразует `AnalyticsReportResponse` в Word документ
3. Возвращается файл для скачивания

---

## Архитектурные паттерны

### ✅ CQRS (Command Query Responsibility Segregation)
- Разделение чтения (Query) и записи (Command)
- `GetAnalyticsReportQuery` - read-only операция

### ✅ Vertical Slice Architecture
- Все компоненты для одной фичи в одной папке
- `src/Application/Reports/Queries/GetAnalytics/`

### ✅ Dependency Injection
- Все зависимости инжектируются через конструктор
- Легко тестируется и расширяется

### ✅ Result Pattern
- `Result<T>` вместо exceptions для бизнес-логики
- Явная обработка ошибок

### ✅ Separation of Concerns
- **SubmissionQueryBuilder:** Построение запросов к БД
- **QuestionAggregator:** Агрегация данных
- **MetricCalculator:** Расчет метрик
- **ResponseMapper:** Маппинг в DTO
- **AnalyticsReportBuilder:** Оркестрация всего процесса

---

## Производительность

### Оптимизации:
- ✅ `AsNoTracking()` - отключение change tracking для read-only операций
- ✅ Проекции вместо загрузки полных entity
- ✅ Агрегация на уровне БД (GroupBy в SQL)
- ✅ Параллельная обработка срезов возможна (но сейчас последовательно)

### Потенциальные улучшения:
- 🔄 Кэширование результатов для популярных запросов
- 🔄 Параллельная обработка срезов (`Task.WhenAll`)
- 🔄 Пагинация для больших отчетов

---

## Обработка ошибок

### Возможные ошибки:
1. **Форма не найдена:** `FormErrors.NotFound(formId)`
2. **Нет срезов:** `Error.Validation("Analytics.SlicesRequired", ...)`
3. **Ошибки авторизации:** 401 Unauthorized
4. **Ошибки валидации:** 400 Bad Request

### Формат ошибки:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Analytics.SlicesRequired": ["At least one analytics slice is required."]
  }
}
```

---

## Диаграмма последовательности

```
UI                API              Handler           Builder           Database
│                  │                  │                 │                 │
├─POST /analytics─>│                  │                 │                 │
│                  ├─Handle()────────>│                 │                 │
│                  │                  ├─BuildAsync()───>│                 │
│                  │                  │                 ├─LoadForm()────>│
│                  │                  │                 │<────Form────────┤
│                  │                  │                 │                 │
│                  │                  │                 ├─BuildSlice()───>│
│                  │                  │                 │  ├─BuildQuery()>│
│                  │                  │                 │  ├─Count()─────>│
│                  │                  │                 │  ├─Aggregate()─>│
│                  │                  │                 │<─Aggregates─────┤
│                  │                  │                 ├─Calculate()     │
│                  │                  │                 ├─MapResponse()   │
│                  │                  │<─Response───────┤                 │
│                  │<─Result──────────┤                 │                 │
│<─200 OK + JSON───┤                  │                 │                 │
│                  │                  │                 │                 │
```

---

## Резюме

**User Flow в одном предложении:**
Пользователь отправляет POST запрос с FormId и срезами → API валидирует и делегирует в Handler → Builder загружает форму, для каждого среза строит запрос к БД, агрегирует ответы, рассчитывает метрики → Mapper преобразует в DTO → UI получает JSON с аналитикой для отображения графиков и таблиц.

**Ключевые компоненты:**
1. **API Endpoint** - точка входа
2. **Query Handler** - валидация и делегирование
3. **Report Builder** - оркестрация
4. **Query Builder** - фильтрация данных
5. **Aggregator** - агрегация в БД
6. **Calculator** - расчет метрик
7. **Mapper** - преобразование в DTO
