# Сравнительный анализ архитектур: Clean Architecture Template vs Questionnaire Project

## Обзор

Этот документ содержит детальное сравнение двух реализаций Clean Architecture:
- **Clean Architecture Template** (Milan Jovanović) - эталонная реализация
- **Questionnaire Project** (dotnet-questionnaire-vfbsac) - текущий проект

---

## 1. Структура проектов

### Clean Architecture Template
```
src/
├── SharedKernel/              # Общие абстракции и базовые классы
│   ├── Entity.cs
│   ├── Result.cs
│   ├── Error.cs
│   ├── IDateTimeProvider.cs
│   └── IDomainEvent.cs
├── Domain/                    # Доменный слой
│   ├── Todos/
│   └── Users/
├── Application/               # Слой приложения
│   ├── Abstractions/
│   │   ├── Authentication/    # IPasswordHasher, ITokenProvider, IUserContext
│   │   ├── Behaviors/         # Декораторы
│   │   ├── Data/              # IApplicationDbContext
│   │   └── Messaging/         # CQRS интерфейсы
│   └── [Features]/            # Команды и запросы
├── Infrastructure/            # Инфраструктурный слой
│   ├── Authentication/
│   ├── Authorization/        # Permission-based
│   ├── Database/
│   ├── DomainEvents/
│   └── Time/
└── Web.Api/                   # API слой
    ├── Endpoints/             # Minimal APIs
    ├── Infrastructure/
    └── Middleware/
```

### Questionnaire Project
```
src/
├── Questionnaire.SharedKernel/
├── Questionnaire.Domain/
├── Questionnaire.Application/
│   ├── Abstractions/
│   │   ├── Behaviors/
│   │   └── Messaging/
│   └── [Features]/
├── Questionnaire.Infrastructure/
│   ├── Authentication/
│   ├── Persistence/
│   └── Services/
├── Questionnaire.Api/         # Controllers
└── Questionnaire.Contracts/    # DTO
```

**Ключевые отличия:**
- ✅ Template использует **Minimal APIs** (Endpoints), проект - **Controllers**
- ✅ Template имеет **Permission-based Authorization**
- ✅ Template организует абстракции в `Application.Abstractions.Authentication`
- ✅ Template использует **snake_case naming** для PostgreSQL

---

## 2. Обработка ошибок

### Оба проекта используют Custom Result<T>

**Структура идентична:**
```csharp
public class Result<TValue> : Result
{
    public TValue Value { get; }
    public static Result<TValue> Success(TValue value);
    public static Result<TValue> Failure(Error error);
}
```

**Статус:** ✅ Полное соответствие

---

## 3. CQRS реализация

### Оба проекта используют Custom CQRS (не MediatR)

**Интерфейсы идентичны:**
```csharp
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}
```

**Статус:** ✅ Полное соответствие

---

## 4. Валидация

### Clean Architecture Template
- ✅ Валидирует только **Commands** (через ValidationDecorator)
- ❌ **Queries НЕ валидируются** (только логирование)

```csharp
// Только Commands
services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

// Queries только логируются
services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
```

### Questionnaire Project
- ✅ Валидирует **Commands**
- ⚠️ **Валидирует Queries** (отличается от шаблона)

```csharp
// Commands и Queries валидируются
services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationDecorator.QueryHandler<,>));
```

**Рекомендация:** Убрать валидацию для Queries (следуя шаблону)

---

## 5. Логирование

### Оба проекта используют LoggingDecorator

**Статус:** ✅ Полное соответствие

---

## 6. Domain Events

### Clean Architecture Template

**ApplicationDbContext:**
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    int result = await base.SaveChangesAsync(cancellationToken);
    await PublishDomainEventsAsync();  // ПОСЛЕ SaveChanges
    return result;
}

private async Task PublishDomainEventsAsync()
{
    var domainEvents = ChangeTracker
        .Entries<Entity>()
        .Select(entry => entry.Entity)
        .SelectMany(entity =>
        {
            List<IDomainEvent> domainEvents = entity.DomainEvents;
            entity.ClearDomainEvents();  // Очистка сразу
            return domainEvents;
        })
        .ToList();

    await domainEventsDispatcher.DispatchAsync(domainEvents);
}
```

**DomainEventsDispatcher:**
```csharp
public async Task DispatchAsync(
    IEnumerable<IDomainEvent> domainEvents,
    CancellationToken cancellationToken = default)
{
    foreach (IDomainEvent domainEvent in domainEvents)
    {
        using IServiceScope scope = serviceProvider.CreateScope();  // ✅ Scoped для каждого события
        
        // Обработка события в отдельном scope
    }
}
```

**Преимущества:**
- ✅ Каждое событие обрабатывается в отдельном scope
- ✅ События не зависят от основного DbContext
- ✅ Лучшая изоляция и обработка ошибок

### Questionnaire Project

**ApplicationDbContext:**
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    int result = await base.SaveChangesAsync(cancellationToken);
    await domainEventsDispatcher.DispatchDomainEventsAsync(cancellationToken);
    return result;
}
```

**DomainEventsDispatcher:**
```csharp
public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
{
    List<Entity> entities = dbContext.ChangeTracker
        .Entries<Entity>()
        .Where(entry => entry.Entity.DomainEvents.Count != 0)
        .Select(entry => entry.Entity)
        .ToList();

    List<IDomainEvent> domainEvents = entities
        .SelectMany(entity => entity.DomainEvents)
        .ToList();

    entities.ForEach(entity => entity.ClearDomainEvents());

    foreach (IDomainEvent domainEvent in domainEvents)
    {
        await PublishDomainEventAsync(domainEvent, cancellationToken);
        // ❌ Использует основной serviceProvider (не scoped)
    }
}
```

**Проблемы:**
- ❌ Использует основной `serviceProvider` вместо scoped
- ❌ Все события обрабатываются в одном контексте
- ❌ Меньшая изоляция

**Рекомендация:** Применить подход из шаблона (scoped services для каждого события)

---

## 7. ApplicationDbContext

### Clean Architecture Template
```csharp
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);  // ✅ Явная схема
    }
    
    // Использует UseSnakeCaseNamingConvention()  // ✅ snake_case
}
```

### Questionnaire Project
```csharp
internal sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher) 
    : DbContext(options), IApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        // ❌ Нет явной схемы
        // ❌ Нет snake_case naming
    }
}
```

**Рекомендация:** Добавить схему и snake_case naming для PostgreSQL

---

## 8. GlobalExceptionHandler

### Clean Architecture Template
```csharp
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(...)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Server failure"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Questionnaire Project
```csharp
internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(...)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        ProblemDetails problemDetails = new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = exception.GetType().Name,  // ⚠️ Отличается
            Title = "An error occurred while processing your request.",
            Detail = exception.Message,  // ⚠️ Включает детали
            Instance = httpContext.Request.Path
        };
        // ...
    }
}
```

**Различия:**
- Template использует стандартный RFC тип ошибки
- Template не раскрывает детали исключения (безопаснее)
- Template использует `Microsoft.AspNetCore.Mvc.ProblemDetails`

**Рекомендация:** Унифицировать с шаблоном для безопасности

---

## 9. Dependency Injection

### Clean Architecture Template

**Application:**
```csharp
services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
    // ...
);
```

**Infrastructure:**
```csharp
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();  // ✅ Transient
```

### Questionnaire Project

**Application:**
```csharp
// Отдельные Scan для каждого типа
services.Scan(scan => scan
    .FromAssembliesOf(typeof(DependencyInjection))
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
    // ...
);
```

**Infrastructure:**
```csharp
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();  // ⚠️ Scoped
```

**Различия:**
- Template использует `Transient` для DomainEventsDispatcher (лучше для scoped обработки)
- Template объединяет несколько Scan в один

**Рекомендация:** Использовать Transient для DomainEventsDispatcher

---

## 10. API Endpoints

### Clean Architecture Template: Minimal APIs
```csharp
internal sealed class Complete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("todos/{id:guid}/complete", async (
            Guid id,
            ICommandHandler<CompleteTodoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteTodoCommand(id);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
```

**Преимущества:**
- ✅ Меньше boilerplate
- ✅ Один файл = один endpoint
- ✅ Лучшая производительность
- ✅ Автоматическая регистрация через `IEndpoint`

### Questionnaire Project: Controllers
```csharp
[ApiController]
[Route("forms")]
public class FormsController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateForm(CreateFormRequest request)
    {
        // ...
    }
}
```

**Преимущества:**
- ✅ Привычный подход
- ✅ Хорошая поддержка инструментов

**Рекомендация:** Оставить Controllers (приемлемый выбор)

---

## 11. Структура Application.Abstractions

### Clean Architecture Template
```
Application.Abstractions/
├── Authentication/
│   ├── IPasswordHasher.cs
│   ├── ITokenProvider.cs
│   └── IUserContext.cs
├── Behaviors/
├── Data/
└── Messaging/
```

### Questionnaire Project
```
Application.Abstractions/
├── Behaviors/
├── Messaging/
└── IDomainEventsDispatcher.cs

Application.Common.Interfaces/
├── IApplicationDbContext.cs
├── ICurrentUserProvider.cs
├── IJwtTokenGenerator.cs
├── IPasswordHasher.cs
└── IReportGenerator.cs
```

**Различия:**
- Template группирует по функциональности (Authentication, Data)
- Проект использует `Common.Interfaces` для всех интерфейсов
- Template использует `IUserContext`, проект - `ICurrentUserProvider`

**Рекомендация:** Рассмотреть реорганизацию по функциональности (опционально)

---

## 12. Health Checks

### Clean Architecture Template
```csharp
services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("Database")!);

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Questionnaire Project
```csharp
services.AddHealthChecks()
    .AddNpgsql(connectionString ?? string.Empty, name: "database");

app.MapHealthChecks("/health");
```

**Статус:** ✅ Оба реализованы, различия минимальны

---

## 13. Миграции

### Clean Architecture Template
```csharp
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();  // ✅ Расширение метода
}
```

### Questionnaire Project
```csharp
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();  // ✅ Async версия
}
```

**Статус:** ✅ Оба реализованы

---

## Сводная таблица соответствия

| Компонент | Template | Questionnaire | Статус |
|-----------|----------|---------------|--------|
| **Result<T> Pattern** | ✅ | ✅ | ✅ Соответствует |
| **Custom CQRS** | ✅ | ✅ | ✅ Соответствует |
| **ValidationDecorator** | Commands only | Commands + Queries | ⚠️ Отличается |
| **LoggingDecorator** | ✅ | ✅ | ✅ Соответствует |
| **Domain Events** | Scoped per event | Single scope | ⚠️ Нужно улучшить |
| **ApplicationDbContext** | Schema + snake_case | Базовый | ⚠️ Можно улучшить |
| **GlobalExceptionHandler** | RFC standard | Custom | ⚠️ Можно улучшить |
| **IDomainEventsDispatcher** | Transient | Scoped | ⚠️ Нужно изменить |
| **IDateTimeProvider** | ✅ | ✅ | ✅ Соответствует |
| **RequestContextLogging** | ✅ | ✅ | ✅ Соответствует |
| **Health Checks** | ✅ | ✅ | ✅ Соответствует |
| **API Style** | Minimal APIs | Controllers | ✅ Оба валидны |
| **Permission Auth** | ✅ | ❌ | ⚠️ Опционально |

---

## Рекомендации по улучшению

### Приоритет 1 (Критично)
1. ✅ **Улучшить DomainEventsDispatcher** - использовать scoped services для каждого события
2. ✅ **Изменить IDomainEventsDispatcher на Transient** - для правильной работы scoped обработки

### Приоритет 2 (Важно)
3. ✅ **Убрать валидацию для Queries** - следовать шаблону (queries не валидируются)
4. ✅ **Улучшить GlobalExceptionHandler** - использовать стандартный RFC ProblemDetails
5. ✅ **Добавить схему и snake_case naming** в ApplicationDbContext

### Приоритет 3 (Улучшения)
6. ⚠️ **Реорганизовать Application.Abstractions** - группировка по функциональности (опционально)
7. ⚠️ **Рассмотреть Permission-based Authorization** - для более гибкой системы прав (опционально)

---

## Заключение

Проект **dotnet-questionnaire-vfbsac** в целом хорошо следует принципам Clean Architecture и имеет много общего с эталонным шаблоном. Основные различия:

1. **Domain Events** - нужна доработка для использования scoped services
2. **Валидация Queries** - убрать (следовать шаблону)
3. **GlobalExceptionHandler** - унифицировать с шаблоном
4. **ApplicationDbContext** - добавить схему и naming convention

Большинство улучшений можно применить без изменения бизнес-логики, что улучшит соответствие шаблону и упростит поддержку кода.
