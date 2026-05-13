# Word Export Endpoints

## Обзор

Добавлена функциональность экспорта аналитических отчетов в формат Word (.docx) через новые API эндпоинты.

## Backend Changes

### Новые эндпоинты

Созданы три новых эндпоинта для экспорта отчетов:

1. **POST** `/reports/analytics/period/export`
   - Экспорт аналитики за один период
   - Принимает: `ExportAnalyticsByPeriodCommand`
   - Возвращает: Word документ (.docx)

2. **POST** `/reports/analytics/groups/export`
   - Экспорт сравнительной аналитики по группам
   - Принимает: `ExportAnalyticsByGroupsCommand`
   - Возвращает: Word документ (.docx)

3. **POST** `/reports/analytics/periods/export`
   - Экспорт сравнительной аналитики по периодам
   - Принимает: `ExportAnalyticsByPeriodsCommand`
   - Возвращает: Word документ (.docx)

### Файлы

- `src/Web.Api/Endpoints/Reports/ExportAnalyticsByPeriod.cs`
- `src/Web.Api/Endpoints/Reports/ExportAnalyticsByGroups.cs`
- `src/Web.Api/Endpoints/Reports/ExportAnalyticsByPeriods.cs`

### Dependency Injection

Зарегистрирован сервис `IWordReportGenerator` в `Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<IWordReportGenerator, WordReportGenerator>();
```

## Frontend Changes

### API Client

Добавлены три новых метода в `src/Web.Client/src/api.ts`:

```typescript
exportAnalyticsByPeriod: (payload: AnalyticsByPeriodRequest) =>
  api.post("/reports/analytics/period/export", payload, {
    responseType: "blob",
  }),

exportAnalyticsByPeriods: (payload: GetAnalyticsByPeriodsRequest) =>
  api.post("/reports/analytics/periods/export", payload, {
    responseType: "blob",
  }),

exportAnalyticsByGroups: (payload: GetAnalyticsByGroupsRequest) =>
  api.post("/reports/analytics/groups/export", payload, {
    responseType: "blob",
  }),
```

### UI Changes

Обновлена функция `exportReport` в `src/Web.Client/src/pages/admin/admin-stats-page.tsx`:

- Автоматически определяет режим аналитики (single/periods/groups)
- Вызывает соответствующий API метод
- Скачивает Word документ с датой в имени файла
- Показывает уведомления об успехе/ошибке

## Использование

1. Откройте страницу аналитики формы
2. Настройте параметры отчета (период, фильтры, режим)
3. Нажмите кнопку "Экспорт в Word"
4. Word документ автоматически скачается

## Форматы имен файлов

- Один период: `analytics-period-YYYY-MM-DD.docx`
- Несколько периодов: `analytics-periods-YYYY-MM-DD.docx`
- Сравнение групп: `analytics-groups-YYYY-MM-DD.docx`

## Технические детали

- Все эндпоинты требуют авторизации
- Используется MIME тип: `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- Генерация документов выполняется через `IWordReportGenerator` (OpenXML)
- Фронтенд использует Blob API для скачивания файлов
