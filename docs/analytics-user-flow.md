# User Flow: Отчеты Аналитики (UI → Backend)

## Обзор
Этот документ описывает полный поток данных от пользовательского интерфейса до backend для генерации отчетов аналитики по анкетам.

Система предоставляет 4 специализированных endpoint для разных типов аналитики:
1. **Advices** - текстовые ответы (советы/комментарии)
2. **Analytics by Period** - статистика за один период
3. **Analytics by Periods** - сравнение нескольких периодов
4. **Analytics by Groups** - группировка по измерениям (отдел/дисциплина/преподаватель)

---

## 1. Endpoints Overview

### 1.1 Get Advices (Текстовые ответы)
```http
GET /reports/forms/{formId}/advices?teacherId={guid}
Authorization: Bearer {token}
```

**Параметры:**
- `formId` (path, required) - ID формы
- `teacherId` (query, optional) - фильтр по преподавателю

**Response:**
```json
[
  {
    "text": "Отличный курс, много практики",
    "teacherId": "guid-or-null",
    "departmentId": "guid-or-null"
  }
]
```

### 1.2 Analytics by Period (Один период)
```http
POST /reports/analytics/period
Authorization: Bearer {token}
Content-Type: application/json

{
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromDate": "2026-03-01T00:00:00Z",
  "toDate": "2026-05-31T23:59:59Z",
  "filterSet": {
    "disciplineId": "guid-optional",
    "teacherId": "guid-optional",
    "departmentId": "guid-optional",
    "specialityId": "guid-optional",
    "specializationId": "guid-optional",
    "organizationName": "optional-string",
    "educationForm": "optional-string",
    "employeeCategory": "optional-string",
    "position": "optional-string"
  }
}
```

**Response:**
```json
[
  {
    "questionId": "guid",
    "questionText": "Оцените качество материала",
    "median": 8.5,
    "mean": 8.3,
    "mode": 9.0,
    "standardDeviation": 1.2,
    "responseCount": 150
  }
]
```

### 1.3 Analytics by Periods (Сравнение периодов)
```http
POST /reports/analytics/periods
Authorization: Bearer {token}
Content-Type: application/json

{
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "periods": [
    {
      "label": "Весна 2026",
      "dateFrom": "2026-03-01T00:00:00Z",
      "dateTo": "2026-05-31T23:59:59Z",
      "filterSet": {
        "disciplineId": "guid-optional",
        "teacherId": "guid-optional"
      }
    },
    {
      "label": "Осень 2025",
      "dateFrom": "2025-09-01T00:00:00Z",
      "dateTo": "2025-11-30T23:59:59Z",
      "filterSet": {}
    }
  ]
}
```

**Response:**
```json
[
  {
    "label": "Весна 2026",
    "periodStart": "2026-03-01T00:00:00Z",
    "periodEnd": "2026-05-31T23:59:59Z",
    "questionStatistics": [
      {
        "questionId": "guid",
        "questionText": "Оцените качество материала",
        "median": 8.5,
        "mean": 8.3,
        "mode": 9.0,
        "standardDeviation": 1.2,
        "responseCount": 150
      }
    ]
  },
  {
    "label": "Осень 2025",
    "periodStart": "2025-09-01T00:00:00Z",
    "periodEnd": "2025-11-30T23:59:59Z",
    "questionStatistics": [...]
  }
]
```

### 1.4 Analytics by Groups (Группировка)
```http
POST /reports/analytics/groups
Authorization: Bearer {token}
Content-Type: application/json

{
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fromDate": "2026-03-01T00:00:00Z",
  "toDate": "2026-05-31T23:59:59Z",
  "groupBy": "Department",
  "filterSet": {
    "disciplineId": "guid-optional"
  }
}
```

**GroupBy values:**
- `Department`
- `Discipline`
- `Speciality`
- `Specialization`
- `EducationForm`
- `EmployeeCategory`
- `Teacher`

**Response:**
```json
[
  {
    "groupKey": "guid-or-string",
    "groupName": "Department guid",
    "questionStatistics": [
      {
        "questionId": "guid",
        "questionText": "Оцените качество материала",
        "median": 8.5,
        "mean": 8.3,
        "mode": 9.0,
        "standardDeviation": 1.2,
        "responseCount": 150
      }
    ]
  }
]
```


---

## 2. Common Flow: API → Application Layer

All analytics endpoints follow same pattern:

### Endpoint Structure
```csharp
app.MapPost("reports/analytics/{type}", async (
    Query query,
    IQueryHandler<Query, Response> handler,
    CancellationToken cancellationToken) =>
{
    Result<Response> result = await handler.Handle(query, cancellationToken);
    return result.Match(Results.Ok, CustomResults.Problem);
})
.WithTags("Reports")
.RequireAuthorization();
```

**Common steps:**
1. ASP.NET deserialize JSON to Query object
2. Endpoint require JWT authorization
3. Delegate to Application Layer via CQRS pattern
4. Return Result (success or error)

---

## 3. Query Handlers

### 3.1 GetAdvicesQueryHandler
**Location:** `src/Application/Reports/Queries/GetAdvices/`

**Logic:**
1. Verify form exist
2. Build query: filter by formId + optional teacherId
3. Select text answers (where `Value != null`)
4. Return list of text responses with context

**Key code:**
```csharp
List<GetAdvicesQueryResponse> responses = await submissionsQuery
    .SelectMany(s => s.Answers, (submission, answer) => new { submission, answer })
    .Where(x => x.answer.Value != null && x.answer.Value != "")
    .Select(x => new GetAdvicesQueryResponse(
        x.answer.Value!,
        x.submission.Context.TeacherId,
        x.submission.Context.DepartmentId))
    .ToListAsync(cancellationToken);
```

### 3.2 GetAnalyticsByPeriodQueryHandler
**Location:** `src/Application/Reports/Queries/GetAnalyticsByPeriod/`

**Logic:**
1. Verify form exist
2. Apply filters via `SubmissionFilterHelper`
3. Normalize date range: `ToDate.AddDays(1)` for inclusive end
4. Get numeric answers grouped by question
5. Calculate statistics (median, mean, mode, stddev) in-memory
6. Return question statistics

**Date normalization:**
```csharp
DateTime normalizedToDate = query.ToDate.AddDays(1);
filteredByDate = filteredQuery
    .Where(s => s.SubmittedAt >= query.FromDate && s.SubmittedAt < normalizedToDate);
```

### 3.3 GetAnalyticsByPeriodsQueryHandler
**Location:** `src/Application/Reports/Queries/GetAnalyticsByPeriods/`

**Logic:**
1. Verify form exist
2. For each period:
   - Build submission query with filters
   - Normalize date range
   - Get submission IDs (optimized - no full entity load)
   - Get numeric answers grouped by question
   - Calculate statistics
3. Return list of period results

**Optimization:**
```csharp
// Get IDs only, not full Submission entities
List<Guid> submissionIds = await submissionsQuery
    .Select(s => s.Id)
    .ToListAsync(cancellationToken);
```

### 3.4 GetAnalyticsByGroupsQueryHandler
**Location:** `src/Application/Reports/Queries/GetAnalyticsByGroups/`

**Logic:**
1. Verify form exist
2. Apply filters + date range
3. Project only needed fields (ID + grouping fields) - optimized
4. Group submissions by dimension (Department/Discipline/Teacher/etc)
5. Get numeric answers for all submissions
6. Calculate statistics per group
7. Return group statistics

**Optimization:**
```csharp
// Project only needed fields, not full entity
var submissionsWithGrouping = await filteredByDate
    .Select(s => new
    {
        s.Id,
        s.Context.DepartmentId,
        s.Context.DisciplineId,
        // ... other grouping fields
    })
    .ToListAsync(cancellationToken);
```

---

## 4. Shared Components

### 4.1 SubmissionFilterHelper
**Location:** `src/Application/Reports/Queries/Shared/SubmissionFilterHelper.cs`

Apply filters to submission query:
```csharp
public static IQueryable<Submission> ApplyFilters(
    IQueryable<Submission> query,
    AnalyticsFilterSet filterSet)
{
    if (filterSet.DepartmentId.HasValue)
        query = query.Where(s => s.Context.DepartmentId == filterSet.DepartmentId);
    
    if (filterSet.DisciplineId.HasValue)
        query = query.Where(s => s.Context.DisciplineId == filterSet.DisciplineId);
    
    // ... other filters
    
    return query;
}
```

### 4.2 StatisticsCalculator
**Location:** `src/Application/Reports/Queries/Shared/StatisticsCalculator.cs`

Calculate statistical metrics:
- **Median:** Middle value when sorted
- **Mean:** Average (Σx / n)
- **Mode:** Most frequent value
- **Standard Deviation:** √(Σ(x - μ)² / n)

### 4.3 QuestionStatistics (Shared DTO)
**Location:** `src/Application/Reports/Queries/Shared/QuestionStatistics.cs`

```csharp
public sealed record QuestionStatistics(
    Guid QuestionId,
    string QuestionText,
    decimal Median,
    decimal Mean,
    decimal Mode,
    decimal StandardDeviation,
    int ResponseCount);
```

Used by both `GetAnalyticsByPeriods` and `GetAnalyticsByGroups`.

---

## 5. Database Query Patterns

### Pattern 1: Filter in DB, Calculate in Memory
All analytics queries follow this pattern:

```csharp
// Step 1: Filter in database
var answersGrouped = await context.Answers
    .AsNoTracking()
    .Where(a => submissionIds.Contains(a.SubmissionId) &&
               a.Value == null &&
               a.NumericValue != null)
    .GroupBy(a => a.QuestionId)
    .Select(g => new
    {
        QuestionId = g.Key,
        Values = g.Select(a => a.NumericValue!.Value).ToList()
    })
    .ToListAsync(cancellationToken);

// Step 2: Calculate statistics in-memory
var stats = answersGrouped
    .Select(group => new QuestionStatistics(
        QuestionId: group.QuestionId,
        QuestionText: questions[group.QuestionId],
        Median: StatisticsCalculator.CalculateMedian(group.Values),
        Mean: StatisticsCalculator.CalculateMean(group.Values),
        Mode: StatisticsCalculator.CalculateMode(group.Values),
        StandardDeviation: StatisticsCalculator.CalculateStandardDeviation(group.Values),
        ResponseCount: group.Values.Count))
    .ToList();
```

**Why?** Median, mode, stddev not available in SQL. Fetch values, calculate in C#.

### Pattern 2: Projection over Full Entity Load
```csharp
// BAD: Load full entities
List<Submission> submissions = await query.ToListAsync();

// GOOD: Project only needed fields
var data = await query
    .Select(s => new { s.Id, s.Context.DepartmentId })
    .ToListAsync();
```

### Pattern 3: Date Range Normalization
```csharp
// User input: "2026-05-31" (end of day)
// Normalize: Add 1 day, use exclusive upper bound
DateTime normalizedToDate = query.ToDate.AddDays(1);
query = query.Where(s => s.SubmittedAt >= fromDate && s.SubmittedAt < normalizedToDate);
```

Ensures inclusive end date without time precision issues.

---

## 6. Response Structures

### GetAdvicesQueryResponse
```csharp
public sealed record GetAdvicesQueryResponse(
    string Text,
    Guid? TeacherId,
    Guid? DepartmentId);
```

### GetAnalyticsByPeriodQueryResponse
```csharp
public sealed record GetAnalyticsByPeriodQueryResponse(
    Guid QuestionId,
    string QuestionText,
    decimal Median,
    decimal Mean,
    decimal Mode,
    decimal StandardDeviation,
    int ResponseCount);
```

### GetAnalyticsByPeriodsQueryResponse
```csharp
public sealed record GetAnalyticsByPeriodsQueryResponse(
    string Label,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<QuestionStatistics> QuestionStatistics);
```

### GetAnalyticsByGroupsQueryResponse
```csharp
public sealed record GetAnalyticsByGroupsQueryResponse(
    string GroupKey,
    string GroupName,
    List<QuestionStatistics> QuestionStatistics);
```


---

## 7. UI Integration Examples

### Example 1: Single Period Report
```typescript
// Fetch analytics for one period
const response = await fetch('/reports/analytics/period', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    formId: 'form-guid',
    fromDate: '2026-03-01T00:00:00Z',
    toDate: '2026-05-31T23:59:59Z',
    filterSet: {
      disciplineId: 'discipline-guid'
    }
  })
});

const stats = await response.json();
// stats: Array<QuestionStatistics>
```

### Example 2: Compare Multiple Periods
```typescript
// Compare spring vs fall
const response = await fetch('/reports/analytics/periods', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    formId: 'form-guid',
    periods: [
      {
        label: 'Spring 2026',
        dateFrom: '2026-03-01T00:00:00Z',
        dateTo: '2026-05-31T23:59:59Z',
        filterSet: {}
      },
      {
        label: 'Fall 2025',
        dateFrom: '2025-09-01T00:00:00Z',
        dateTo: '2025-11-30T23:59:59Z',
        filterSet: {}
      }
    ]
  })
});

const periods = await response.json();
// periods: Array<PeriodAnalyticsResponse>
// UI can render comparison charts
```

### Example 3: Group by Department
```typescript
// Compare departments
const response = await fetch('/reports/analytics/groups', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    formId: 'form-guid',
    fromDate: '2026-01-01T00:00:00Z',
    toDate: '2026-12-31T23:59:59Z',
    groupBy: 'Department',
    filterSet: {}
  })
});

const groups = await response.json();
// groups: Array<GroupAnalyticsResponse>
// UI can render bar chart per department
```

### Example 4: Get Text Feedback
```typescript
// Fetch advice/comments
const response = await fetch(
  `/reports/forms/${formId}/advices?teacherId=${teacherId}`,
  {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  }
);

const advices = await response.json();
// advices: Array<{ text, teacherId, departmentId }>
```

---

## 8. Performance Optimizations

### Applied Optimizations:
- ✅ `AsNoTracking()` - disable change tracking for read-only queries
- ✅ Projection instead of full entity load
- ✅ DB-side aggregation (GroupBy in SQL)
- ✅ Date normalization for consistent filtering
- ✅ Shared `QuestionStatistics` DTO to avoid duplication
- ✅ ID-only queries before fetching related data

### Query Optimization Example:
```csharp
// BAD: Load full entities
List<Submission> submissions = await context.Submissions
    .Where(s => s.FormId == formId)
    .ToListAsync();
var ids = submissions.Select(s => s.Id).ToList();

// GOOD: Project IDs only
List<Guid> ids = await context.Submissions
    .Where(s => s.FormId == formId)
    .Select(s => s.Id)
    .ToListAsync();
```

Reduces memory usage and network transfer significantly.

---

## 9. Error Handling

### Possible Errors:

1. **Form not found**
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
     "title": "Not Found",
     "status": 404,
     "detail": "Form with ID 'guid' was not found"
   }
   ```

2. **Unauthorized**
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
     "title": "Unauthorized",
     "status": 401
   }
   ```

3. **Validation error** (e.g., empty periods list)
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
     "title": "Validation Error",
     "status": 400,
     "errors": {
       "Periods": ["At least one period is required"]
     }
   }
   ```

---

## 10. Architecture Patterns

### ✅ CQRS (Command Query Responsibility Segregation)
- Separation of read (Query) and write (Command)
- All analytics are read-only queries

### ✅ Vertical Slice Architecture
- All components for one feature in one folder
- `src/Application/Reports/Queries/{FeatureName}/`

### ✅ Dependency Injection
- All dependencies injected via constructor
- Easy to test and extend

### ✅ Result Pattern
- `Result<T>` instead of exceptions for business logic
- Explicit error handling

### ✅ Separation of Concerns
- **SubmissionFilterHelper:** Build filtered queries
- **StatisticsCalculator:** Calculate metrics
- **Query Handlers:** Orchestrate data flow
- **Endpoints:** HTTP layer only

---

## 11. Sequence Diagrams

### Single Period Analytics
```
UI                API              Handler           Database
│                  │                  │                 │
├─POST /period────>│                  │                 │
│                  ├─Handle()────────>│                 │
│                  │                  ├─Verify form───>│
│                  │                  ├─Apply filters─>│
│                  │                  ├─Get answers───>│
│                  │                  │<─Values─────────┤
│                  │                  ├─Calculate stats │
│                  │<─Response────────┤                 │
│<─200 OK + JSON───┤                  │                 │
```

### Multi-Period Comparison
```
UI                API              Handler           Database
│                  │                  │                 │
├─POST /periods───>│                  │                 │
│                  ├─Handle()────────>│                 │
│                  │                  ├─Verify form───>│
│                  │                  │                 │
│                  │                  ├─For each period:│
│                  │                  │  ├─Filter─────>│
│                  │                  │  ├─Get IDs────>│
│                  │                  │  ├─Get answers>│
│                  │                  │  ├─Calculate   │
│                  │                  │                 │
│                  │<─Periods─────────┤                 │
│<─200 OK + JSON───┤                  │                 │
```

### Group Analytics
```
UI                API              Handler           Database
│                  │                  │                 │
├─POST /groups────>│                  │                 │
│                  ├─Handle()────────>│                 │
│                  │                  ├─Verify form───>│
│                  │                  ├─Project fields>│
│                  │                  ├─Group by dim   │
│                  │                  ├─Get answers───>│
│                  │                  ├─Calc per group │
│                  │<─Groups──────────┤                 │
│<─200 OK + JSON───┤                  │                 │
```

---

## 12. Summary

**4 Specialized Endpoints:**
1. **GET /reports/forms/{formId}/advices** - text feedback
2. **POST /reports/analytics/period** - single period stats
3. **POST /reports/analytics/periods** - multi-period comparison
4. **POST /reports/analytics/groups** - group by dimension

**Key Components:**
1. **API Endpoints** - HTTP layer
2. **Query Handlers** - validation + orchestration
3. **SubmissionFilterHelper** - filter builder
4. **StatisticsCalculator** - metric calculations
5. **Shared DTOs** - `QuestionStatistics`, `AnalyticsFilterSet`

**Design Principles:**
- Separate endpoints for different use cases
- Shared components for common logic
- DB-side filtering, in-memory calculations
- Optimized queries (projections, no tracking)
- Consistent date handling across all endpoints
