# Детальный план миграции на Template архитектуру

## Общая информация

**Цель:** Миграция проекта `dotnet-questionnaire-vfbsac` с MediatR + ErrorOr на Custom CQRS + Result Pattern по образцу template от Milan Jovanović.

**Подход:** Постепенная миграция по фазам с возможностью тестирования на каждом этапе.

**Оценка времени:** ~2-3 недели для полной миграции (зависит от размера команды).

---

## Подготовка

### Шаг 0.1: Создать ветку для миграции
```bash
git checkout -b feature/migration-to-template-architecture
```

### Шаг 0.2: Установить необходимые NuGet пакеты
```bash
# В корневой директории решения
dotnet add src/Questionnaire.Application/Questionnaire.Application.csproj package Scrutor
dotnet add src/Questionnaire.Application/Questionnaire.Application.csproj package Serilog
dotnet add src/Questionnaire.Application/Questionnaire.Application.csproj package Serilog.Extensions.Logging
```

### Шаг 0.3: Создать резервную копию
```bash
git commit -am "Backup before migration"
```

---

## Фаза 1: Фундамент (SharedKernel + Entity Base)

**Цель:** Создать базовую инфраструктуру для Result pattern и Domain Events.

**Время:** 1-2 дня

### Шаг 1.1: Создать проект SharedKernel

#### 1.1.1. Создать файл проекта
**Файл:** `src/Questionnaire.SharedKernel/Questionnaire.SharedKernel.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

#### 1.1.2. Добавить проект в решение
```bash
dotnet sln add src/Questionnaire.SharedKernel/Questionnaire.SharedKernel.csproj
```

### Шаг 1.2: Создать класс Error

**Файл:** `src/Questionnaire.SharedKernel/Error.cs`
```csharp
namespace Questionnaire.SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);
}
```

**Файл:** `src/Questionnaire.SharedKernel/ErrorType.cs`
```csharp
namespace Questionnaire.SharedKernel;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Problem = 4
}
```

### Шаг 1.3: Создать класс Result<T>

**Файл:** `src/Questionnaire.SharedKernel/Result.cs`
```csharp
using System.Diagnostics.CodeAnalysis;

namespace Questionnaire.SharedKernel;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
```

### Шаг 1.4: Создать ValidationError

**Файл:** `src/Questionnaire.SharedKernel/ValidationError.cs`
```csharp
namespace Questionnaire.SharedKernel;

public class ValidationError : Error
{
    public ValidationError(Error[] errors) : base(
        "Validation.General",
        "One or more validation errors occurred",
        ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}
```

### Шаг 1.5: Создать Entity base class

**Файл:** `src/Questionnaire.SharedKernel/Entity.cs`
```csharp
namespace Questionnaire.SharedKernel;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

### Шаг 1.6: Создать IDomainEvent

**Файл:** `src/Questionnaire.SharedKernel/IDomainEvent.cs`
```csharp
namespace Questionnaire.SharedKernel;

public interface IDomainEvent;
```

### Шаг 1.7: Создать IDomainEventHandler

**Файл:** `src/Questionnaire.SharedKernel/IDomainEventHandler.cs`
```csharp
namespace Questionnaire.SharedKernel;

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
```

### Шаг 1.8: Обновить Domain проект

#### 1.8.1. Добавить ссылку на SharedKernel
**Файл:** `src/Questionnaire.Domain/Questionnaire.Domain.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Questionnaire.SharedKernel\Questionnaire.SharedKernel.csproj" />
  </ItemGroup>
</Project>
```

#### 1.8.2. Обновить сущность Form
**Файл:** `src/Questionnaire.Domain/Entities/Form.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Entities;

public class Form : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FormQuestion> FormQuestions { get; set; } = new List<FormQuestion>();
    public ICollection<FormRole> FormRoles { get; set; } = new List<FormRole>();
}
```

**Повторить для всех остальных сущностей:**
- `Question.cs`
- `Answer.cs`
- `User.cs`
- `Role.cs`
- И т.д.

**Важно:** Добавить `: Entity` ко всем основным сущностям.

### Шаг 1.9: Создать статические классы для ошибок

**Файл:** `src/Questionnaire.Domain/Forms/FormErrors.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Forms;

public static class FormErrors
{
    public static Error NotFound(int formId) => Error.NotFound(
        "Form.NotFound",
        $"The form with Id = '{formId}' was not found.");

    public static Error AlreadyExists(string name) => Error.Conflict(
        "Form.AlreadyExists",
        $"The form with name '{name}' already exists.");

    public static Error QuestionAlreadyExists(int questionId) => Error.Conflict(
        "Form.QuestionAlreadyExists",
        $"The question with Id = '{questionId}' is already in the form.");

    public static Error QuestionNotFound(int questionId) => Error.NotFound(
        "Form.QuestionNotFound",
        $"The question with Id = '{questionId}' is not found in the form.");
}
```

**Файл:** `src/Questionnaire.Domain/Questions/QuestionErrors.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Questions;

public static class QuestionErrors
{
    public static Error NotFound(int questionId) => Error.NotFound(
        "Question.NotFound",
        $"The question with Id = '{questionId}' was not found.");

    public static Error AlreadyExists(string text) => Error.Conflict(
        "Question.AlreadyExists",
        $"The question with text '{text}' already exists.");
}
```

**Создать аналогичные классы для:**
- `AnswerErrors.cs`
- `UserErrors.cs`
- `AuthenticationErrors.cs` (можно оставить в Application)

### Шаг 1.10: Тестирование Фазы 1

```bash
# Проверить компиляцию
dotnet build

# Запустить тесты (если есть)
dotnet test
```

**Проверка:**
- ✅ Проект компилируется без ошибок
- ✅ Все сущности наследуются от Entity
- ✅ Статические классы ошибок созданы

---

## Фаза 2: CQRS (Замена MediatR)

**Цель:** Заменить MediatR на Custom CQRS интерфейсы.

**Время:** 2-3 дня

### Шаг 2.1: Создать CQRS интерфейсы в Application

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/ICommand.cs`
```csharp
namespace Questionnaire.Application.Abstractions.Messaging;

public interface ICommand;

public interface ICommand<TResponse>;
```

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/ICommandHandler.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
```

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/IQuery.cs`
```csharp
namespace Questionnaire.Application.Abstractions.Messaging;

public interface IQuery<TResponse>;
```

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/IQueryHandler.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

### Шаг 2.2: Обновить Application.csproj

**Файл:** `src/Questionnaire.Application/Questionnaire.Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\Questionnaire.Domain\Questionnaire.Domain.csproj" />
    <ProjectReference Include="..\Questionnaire.SharedKernel\Questionnaire.SharedKernel.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.3.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.10" />
    <PackageReference Include="Scrutor" Version="4.2.2" />
    <PackageReference Include="Serilog" Version="4.1.0" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Шаг 2.3: Мигрировать CreateFormCommand

#### 2.3.1. Обновить команду
**Файл:** `src/Questionnaire.Application/Forms/Commands/Create/CreateFormCommand.cs`
```csharp
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Commands.Create;

public sealed record CreateFormCommand(string Name) : ICommand<Form>;
```

#### 2.3.2. Обновить обработчик
**Файл:** `src/Questionnaire.Application/Forms/Commands/Create/CreateFormCommandHandler.cs`
```csharp
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Forms.Commands.Create;

internal sealed class CreateFormCommandHandler : ICommandHandler<CreateFormCommand, Form>
{
    private readonly IApplicationDbContext _context;

    public CreateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Form>> Handle(CreateFormCommand command, CancellationToken cancellationToken)
    {
        var form = new Form
        {
            Name = command.Name,
            IsActive = true
        };

        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(form);
    }
}
```

### Шаг 2.4: Мигрировать остальные команды

**Паттерн миграции для каждой команды:**

1. **Изменить интерфейс:**
   ```csharp
   // Было:
   public record Command(...) : IRequest<ErrorOr<Response>>;
   
   // Стало:
   public sealed record Command(...) : ICommand<Response>;
   ```

2. **Изменить обработчик:**
   ```csharp
   // Было:
   public class CommandHandler : IRequestHandler<Command, ErrorOr<Response>>
   {
       public async Task<ErrorOr<Response>> Handle(...) { }
   }
   
   // Стало:
   internal sealed class CommandHandler : ICommandHandler<Command, Response>
   {
       public async Task<Result<Response>> Handle(...) { }
   }
   ```

3. **Заменить ErrorOr на Result:**
   ```csharp
   // Было:
   if (entity is null)
       return Error.NotFound("Entity not found.");
   return entity;
   
   // Стало:
   if (entity is null)
       return Result.Failure<Response>(EntityErrors.NotFound(id));
   return Result.Success(entity);
   ```

**Список команд для миграции:**
- ✅ `CreateFormCommand` (пример выше)
- `AddQuestionToFormCommand`
- `DeleteFormCommand`
- `RemoveQuestionFromFormCommand`
- `CreateQuestionCommand`
- `DeleteQuestionCommand`
- `SubmitSurveyCommand`
- `RegisterCommand`
- `LoginQuery` (это Query, см. ниже)

### Шаг 2.5: Мигрировать Query

**Пример: GetAllFormsQuery**

**Файл:** `src/Questionnaire.Application/Forms/Queries/GetAll/GetAllFormsQuery.cs`
```csharp
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Forms;

namespace Questionnaire.Application.Forms.Queries.GetAll;

public sealed record GetAllFormsQuery : IQuery<IEnumerable<FormResponse>>;
```

**Файл:** `src/Questionnaire.Application/Forms/Queries/GetAll/GetAllFormsQueryHandler.cs`
```csharp
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Forms;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Forms.Queries.GetAll;

internal sealed class GetAllFormsQueryHandler : IQueryHandler<GetAllFormsQuery, IEnumerable<FormResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFormsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<FormResponse>>> Handle(GetAllFormsQuery query, CancellationToken cancellationToken)
    {
        var forms = await _context.Forms
            .ToListAsync(cancellationToken);

        var response = forms.Select(f => new FormResponse(
            f.Id,
            f.Name,
            f.IsActive,
            null));

        return Result.Success(response);
    }
}
```

**Список Query для миграции:**
- `GetAllFormsQuery`
- `GetFormByIdQuery`
- `GetAllQuestionsQuery`
- `GetAvailableSurveysQuery`
- `GetSummaryReportQuery`
- `ExportReportQuery`
- `LoginQuery`

### Шаг 2.6: Обновить DependencyInjection

**Файл:** `src/Questionnaire.Application/DependencyInjection.cs`
```csharp
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Регистрация Command Handlers
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Регистрация Query Handlers
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Регистрация Domain Event Handlers
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Регистрация FluentValidation валидаторов
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services;
    }
}
```

### Шаг 2.7: Создать Mediator обертку (временно)

Для постепенной миграции создадим обертку, которая будет работать с новыми интерфейсами, но иметь API как у MediatR.

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/ISender.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions.Messaging;

public interface ISender
{
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
```

**Файл:** `src/Questionnaire.Application/Abstractions/Messaging/Mediator.cs`
```csharp
using Questionnaire.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Questionnaire.Application.Abstractions.Messaging;

internal sealed class Mediator : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<ICommand<TResponse>, TResponse>>();
        return await handler.Handle(command, cancellationToken);
    }

    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")!;
        var result = await (Task<Result>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
        return result;
    }

    public async Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")!;
        var result = await (Task<Result<TResponse>>)handleMethod.Invoke(handler, new object[] { query, cancellationToken })!;
        return result;
    }
}
```

**Обновить DependencyInjection.cs:**
```csharp
services.AddScoped<ISender, Mediator>();
```

### Шаг 2.8: Обновить контроллеры (временно)

**Файл:** `src/Questionnaire.Api/Controllers/FormsController.cs`
```csharp
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Commands.Create;
using Questionnaire.Application.Forms.Queries.GetAll;
// ... другие using

[ApiController]
[Route("forms")]
[Authorize]
public class FormsController : ApiController
{
    private readonly ISender _sender;

    public FormsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateForm(CreateFormRequest request)
    {
        var command = new CreateFormCommand(request.Name);
        var result = await _sender.Send(command);

        return result.Match(
            form => Ok(MapToFormResponse(form)),
            error => Problem(error));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForms()
    {
        var query = new GetAllFormsQuery();
        var result = await _sender.Send(query);

        return result.Match(
            forms => Ok(forms),
            error => Problem(error));
    }
}
```

### Шаг 2.9: Удалить MediatR и ErrorOr

**Файл:** `src/Questionnaire.Application/Questionnaire.Application.csproj`
```xml
<!-- Удалить строки: -->
<!-- <PackageReference Include="ErrorOr" Version="2.0.1" /> -->
<!-- <PackageReference Include="MediatR" Version="13.1.0" /> -->
```

**Удалить все using:**
```csharp
// Удалить:
using ErrorOr;
using MediatR;
```

### Шаг 2.10: Тестирование Фазы 2

```bash
dotnet build
dotnet test
```

**Проверка:**
- ✅ Все команды и запросы мигрированы
- ✅ Контроллеры используют ISender
- ✅ Проект компилируется
- ✅ API работает (протестировать через Swagger)

---

## Фаза 3: Декораторы (Валидация + Логирование)

**Цель:** Добавить автоматическую валидацию и логирование через декораторы.

**Время:** 1-2 дня

### Шаг 3.1: Создать ValidationDecorator

**Файл:** `src/Questionnaire.Application/Abstractions/Behaviors/ValidationDecorator.cs`
```csharp
using FluentValidation;
using FluentValidation.Results;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] validationFailures = await ValidateAsync(command, validators);

            if (validationFailures.Length == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Failure<TResponse>(CreateValidationError(validationFailures));
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] validationFailures = await ValidateAsync(command, validators);

            if (validationFailures.Length == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Failure(CreateValidationError(validationFailures));
        }
    }

    private static async Task<ValidationFailure[]> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));

        ValidationFailure[] validationFailures = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToArray();

        return validationFailures;
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
```

### Шаг 3.2: Создать LoggingDecorator

**Файл:** `src/Questionnaire.Application/Abstractions/Behaviors/LoggingDecorator.cs`
```csharp
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Command}", commandName);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {Command}", commandName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Completed command {Command} with error", commandName);
                }
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Command}", commandName);

            Result result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {Command}", commandName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Completed command {Command} with error", commandName);
                }
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            logger.LogInformation("Processing query {Query}", queryName);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed query {Query}", queryName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Completed query {Query} with error", queryName);
                }
            }

            return result;
        }
    }
}
```

### Шаг 3.3: Обновить DependencyInjection для декораторов

**Файл:** `src/Questionnaire.Application/DependencyInjection.cs`
```csharp
// ... existing code ...

public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // ... existing registration code ...

    // Регистрация декораторов
    services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
    services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

    services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
    services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
    services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));

    return services;
}
```

**Важно:** Порядок декораторов важен! Validation должен быть первым (внешним), затем Logging.

### Шаг 3.4: Настроить Serilog

**Файл:** `src/Questionnaire.Api/appsettings.json`
```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  }
}
```

**Файл:** `src/Questionnaire.Api/Program.cs`
```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настроить Serilog
builder.Host.UseSerilog((context, loggerConfig) => 
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// ... остальной код ...
```

### Шаг 3.5: Тестирование Фазы 3

```bash
dotnet build
dotnet run --project src/Questionnaire.Api
```

**Проверка:**
- ✅ Валидация работает автоматически
- ✅ Логирование команд и запросов работает
- ✅ Логи пишутся в консоль и файл

---

## Фаза 4: Domain Events

**Цель:** Реализовать Domain Events для декoupling операций.

**Время:** 2-3 дня

### Шаг 4.1: Создать Domain Events для Form

**Файл:** `src/Questionnaire.Domain/Forms/Events/FormCreatedEvent.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Forms.Events;

public sealed record FormCreatedEvent(int FormId, string FormName) : IDomainEvent;
```

**Файл:** `src/Questionnaire.Domain/Forms/Events/FormDeletedEvent.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Forms.Events;

public sealed record FormDeletedEvent(int FormId) : IDomainEvent;
```

### Шаг 4.2: Обновить сущность Form для использования Events

**Файл:** `src/Questionnaire.Domain/Entities/Form.cs`
```csharp
using Questionnaire.Domain.Forms.Events;
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Entities;

public class Form : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FormQuestion> FormQuestions { get; set; } = new List<FormQuestion>();
    public ICollection<FormRole> FormRoles { get; set; } = new List<FormRole>();

    public static Form Create(string name)
    {
        var form = new Form
        {
            Name = name,
            IsActive = true
        };
        
        form.Raise(new FormCreatedEvent(form.Id, form.Name));
        return form;
    }

    public void MarkAsDeleted()
    {
        Raise(new FormDeletedEvent(Id));
    }
}
```

### Шаг 4.3: Создать Domain Events Dispatcher

**Файл:** `src/Questionnaire.Infrastructure/DomainEvents/DomainEventsDispatcher.cs`
```csharp
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Questionnaire.SharedKernel;

namespace Questionnaire.Infrastructure.DomainEvents;

internal sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();
    private readonly IServiceProvider _serviceProvider;

    public DomainEventsDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            Type domainEventType = domainEvent.GetType();
            Type handlerType = HandlerTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (object? handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }

                var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);
                await handlerWrapper.Handle(domainEvent, cancellationToken);
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            return (HandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper 
        where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

        public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _handler.Handle((T)domainEvent, cancellationToken);
        }
    }
}
```

**Файл:** `src/Questionnaire.Infrastructure/DomainEvents/IDomainEventsDispatcher.cs`
```csharp
using Questionnaire.SharedKernel;

namespace Questionnaire.Infrastructure.DomainEvents;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
```

### Шаг 4.4: Обновить ApplicationDbContext

**Файл:** `src/Questionnaire.Infrastructure/Persistence/ApplicationDbContext.cs`
```csharp
using MediatR; // Временно, пока не удалим полностью
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;
using Questionnaire.Infrastructure.DomainEvents;
using Questionnaire.SharedKernel;
// ... другие using

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventsDispatcher domainEventsDispatcher)
        : base(options)
    {
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    // ... DbSets ...

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        // Очистить события перед сохранением
        ChangeTracker.Entries<Entity>()
            .ToList()
            .ForEach(entry => entry.Entity.ClearDomainEvents());

        int result = await base.SaveChangesAsync(cancellationToken);

        // Диспетчеризировать события после успешного сохранения
        await _domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    // ... OnModelCreating ...
}
```

### Шаг 4.5: Создать Event Handler (пример)

**Файл:** `src/Questionnaire.Application/Forms/Events/FormCreatedEventHandler.cs`
```csharp
using Microsoft.Extensions.Logging;
using Questionnaire.Domain.Forms.Events;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Forms.Events;

internal sealed class FormCreatedEventHandler : IDomainEventHandler<FormCreatedEvent>
{
    private readonly ILogger<FormCreatedEventHandler> _logger;

    public FormCreatedEventHandler(ILogger<FormCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FormCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Form created: {FormId} - {FormName}",
            domainEvent.FormId,
            domainEvent.FormName);

        // Здесь можно добавить дополнительную логику:
        // - Отправка уведомлений
        // - Обновление кэша
        // - Создание связанных записей
        // и т.д.

        return Task.CompletedTask;
    }
}
```

### Шаг 4.6: Обновить обработчики команд для использования Domain методов

**Файл:** `src/Questionnaire.Application/Forms/Commands/Create/CreateFormCommandHandler.cs`
```csharp
// ... using statements ...

internal sealed class CreateFormCommandHandler : ICommandHandler<CreateFormCommand, Form>
{
    private readonly IApplicationDbContext _context;

    public CreateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Form>> Handle(CreateFormCommand command, CancellationToken cancellationToken)
    {
        // Использовать статический метод Create вместо new
        var form = Form.Create(command.Name);

        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken); // Domain Events будут диспетчеризированы здесь

        return Result.Success(form);
    }
}
```

### Шаг 4.7: Зарегистрировать DomainEventsDispatcher

**Файл:** `src/Questionnaire.Infrastructure/DependencyInjection.cs`
```csharp
// ... existing code ...

services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();
```

### Шаг 4.8: Тестирование Фазы 4

```bash
dotnet build
dotnet run --project src/Questionnaire.Api
```

**Проверка:**
- ✅ Создание Form вызывает FormCreatedEvent
- ✅ Event Handler выполняется после SaveChanges
- ✅ Логи показывают обработку событий

---

## Фаза 5: API Layer (Minimal APIs)

**Цель:** Мигрировать Controllers на Minimal APIs (Endpoints).

**Время:** 2-3 дня

### Шаг 5.1: Создать IEndpoint интерфейс

**Файл:** `src/Questionnaire.Api/Endpoints/IEndpoint.cs`
```csharp
namespace Questionnaire.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

### Шаг 5.2: Создать Tags для группировки

**Файл:** `src/Questionnaire.Api/Endpoints/Tags.cs`
```csharp
namespace Questionnaire.Api.Endpoints;

public static class Tags
{
    public const string Forms = "Forms";
    public const string Questions = "Questions";
    public const string Surveys = "Surveys";
    public const string Reports = "Reports";
    public const string Authentication = "Authentication";
    public const string Admin = "Admin";
}
```

### Шаг 5.3: Создать ResultExtensions

**Файл:** `src/Questionnaire.Api/Extensions/ResultExtensions.cs`
```csharp
using Questionnaire.SharedKernel;
using Questionnaire.Api.Infrastructure;

namespace Questionnaire.Api.Extensions;

public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
}
```

### Шаг 5.4: Создать CustomResults

**Файл:** `src/Questionnaire.Api/Infrastructure/CustomResults.cs`
```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.SharedKernel;

namespace Questionnaire.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.Error == Error.None)
        {
            throw new InvalidOperationException("Can't convert success result to problem.");
        }

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: result.Error.Description,
            statusCode: GetStatusCode(result.Error.Type));
    }

    public static IResult Problem<TValue>(Result<TValue> result)
    {
        return Problem((Result)result);
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Problem => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => "Validation Error",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Problem => "Problem",
            _ => "Error"
        };
}
```

### Шаг 5.5: Создать Endpoint для CreateForm

**Файл:** `src/Questionnaire.Api/Endpoints/Forms/Create.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Questionnaire.Api.Endpoints;
using Questionnaire.Api.Extensions;
using Questionnaire.Api.Infrastructure;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Commands.Create;
using Questionnaire.Contracts.Forms;

namespace Questionnaire.Api.Endpoints.Forms;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("forms", async (
            CreateFormRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateFormCommand(request.Name);
            Result<Domain.Entities.Form> result = await sender.Send(command, cancellationToken);

            return result.Match(
                form => Results.Ok(new FormResponse(form.Id, form.Name, form.IsActive, null)),
                CustomResults.Problem);
        })
        .WithTags(Tags.Forms)
        .RequireAuthorization()
        .RequireAuthorization("AdminPolicy"); // Или использовать [Authorize(Roles = "admin")]
    }
}
```

### Шаг 5.5: Создать EndpointExtensions

**Файл:** `src/Questionnaire.Api/Extensions/EndpointExtensions.cs`
```csharp
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Questionnaire.Api.Endpoints;

namespace Questionnaire.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
```

### Шаг 5.6: Обновить Program.cs

**Файл:** `src/Questionnaire.Api/Program.cs`
```csharp
using System.Reflection;
using Questionnaire.Api;
using Questionnaire.Api.Extensions;
// ... другие using

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => 
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Регистрация Endpoints
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

// ... middleware ...

// Маппинг Endpoints
app.MapEndpoints();

// Оставить MapControllers() для постепенной миграции
app.MapControllers();

app.Run();
```

### Шаг 5.7: Мигрировать остальные endpoints

**Создать endpoints для:**
- `Forms/GetAll.cs`
- `Forms/GetById.cs`
- `Forms/Delete.cs`
- `Forms/AddQuestion.cs`
- `Forms/RemoveQuestion.cs`
- `Questions/Create.cs`
- `Questions/GetAll.cs`
- `Questions/Delete.cs`
- `Surveys/GetAvailable.cs`
- `Surveys/Submit.cs`
- `Reports/GetSummary.cs`
- `Reports/Export.cs`
- `Authentication/Login.cs`
- `Authentication/Register.cs`

**Паттерн для каждого:**
```csharp
internal sealed class EndpointName : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.Map[Method]("route", async (...) =>
        {
            // Логика
        })
        .WithTags(Tags.Xxx)
        .RequireAuthorization();
    }
}
```

### Шаг 5.8: Удалить старые контроллеры

После миграции всех endpoints:
```bash
# Удалить файлы контроллеров
rm src/Questionnaire.Api/Controllers/*.cs
```

**Обновить DependencyInjection.cs:**
```csharp
// Удалить:
// services.AddControllers();
```

**Обновить Program.cs:**
```csharp
// Удалить:
// app.MapControllers();
```

### Шаг 5.9: Тестирование Фазы 5

```bash
dotnet build
dotnet run --project src/Questionnaire.Api
```

**Проверка:**
- ✅ Все endpoints работают через Swagger
- ✅ Авторизация работает
- ✅ Валидация работает
- ✅ Логирование работает

---

## Фаза 6: Инфраструктура (GlobalExceptionHandler)

**Цель:** Добавить централизованную обработку исключений.

**Время:** 1 день

### Шаг 6.1: Создать GlobalExceptionHandler

**Файл:** `src/Questionnaire.Api/Infrastructure/GlobalExceptionHandler.cs`
```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Questionnaire.Api.Infrastructure;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Шаг 6.2: Зарегистрировать GlobalExceptionHandler

**Файл:** `src/Questionnaire.Api/DependencyInjection.cs`
```csharp
// ... existing code ...

public static IServiceCollection AddPresentation(this IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    
    services.AddExceptionHandler<GlobalExceptionHandler>();
    services.AddProblemDetails();

    return services;
}
```

### Шаг 6.3: Использовать ExceptionHandler middleware

**Файл:** `src/Questionnaire.Api/Program.cs`
```csharp
// ... existing code ...

app.UseExceptionHandler(); // Добавить эту строку

app.UseAuthentication();
app.UseAuthorization();
```

### Шаг 6.4: Тестирование Фазы 6

```bash
dotnet build
dotnet run --project src/Questionnaire.Api
```

**Проверка:**
- ✅ Необработанные исключения возвращают ProblemDetails
- ✅ Логи пишутся корректно

---

## Фаза 7: Финальная очистка и оптимизация

**Цель:** Удалить временный код, оптимизировать структуру.

**Время:** 1 день

### Шаг 7.1: Удалить временный Mediator

После полной миграции на Endpoints, можно удалить:
- `src/Questionnaire.Application/Abstractions/Messaging/ISender.cs`
- `src/Questionnaire.Application/Abstractions/Messaging/Mediator.cs`

И использовать напрямую `ICommandHandler` и `IQueryHandler` в Endpoints.

### Шаг 7.2: Обновить Endpoints для прямого использования handlers

**Пример:**
```csharp
internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("forms", async (
            CreateFormRequest request,
            ICommandHandler<CreateFormCommand, Domain.Entities.Form> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateFormCommand(request.Name);
            Result<Domain.Entities.Form> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                form => Results.Ok(new FormResponse(form.Id, form.Name, form.IsActive, null)),
                CustomResults.Problem);
        })
        .WithTags(Tags.Forms)
        .RequireAuthorization();
    }
}
```

### Шаг 7.3: Оптимизировать структуру папок

Переместить файлы в более логичную структуру:
```
src/Questionnaire.Application/
├── Abstractions/
│   ├── Messaging/
│   └── Behaviors/
├── Forms/
│   ├── Commands/
│   ├── Queries/
│   └── Events/
└── ...
```

### Шаг 7.4: Обновить документацию

Обновить README.md с новой архитектурой.

### Шаг 7.5: Финальное тестирование

```bash
# Полная сборка
dotnet build

# Запуск тестов
dotnet test

# Запуск приложения
dotnet run --project src/Questionnaire.Api
```

**Проверка:**
- ✅ Все функции работают
- ✅ Нет ошибок компиляции
- ✅ Нет предупреждений
- ✅ API работает через Swagger
- ✅ Логирование работает
- ✅ Валидация работает
- ✅ Domain Events работают

---

## Чеклист миграции

### Фаза 1: Фундамент
- [ ] Создан проект SharedKernel
- [ ] Создан класс Error
- [ ] Создан класс Result<T>
- [ ] Создан класс Entity
- [ ] Все сущности наследуются от Entity
- [ ] Созданы статические классы ошибок

### Фаза 2: CQRS
- [ ] Созданы интерфейсы ICommand, ICommandHandler, IQuery, IQueryHandler
- [ ] Все команды мигрированы
- [ ] Все запросы мигрированы
- [ ] Обновлен DependencyInjection
- [ ] Удален MediatR
- [ ] Удален ErrorOr

### Фаза 3: Декораторы
- [ ] Создан ValidationDecorator
- [ ] Создан LoggingDecorator
- [ ] Настроен Serilog
- [ ] Декораторы зарегистрированы

### Фаза 4: Domain Events
- [ ] Созданы Domain Events
- [ ] Создан DomainEventsDispatcher
- [ ] Обновлен ApplicationDbContext
- [ ] Созданы Event Handlers

### Фаза 5: API Layer
- [ ] Создан IEndpoint
- [ ] Мигрированы все endpoints
- [ ] Удалены старые контроллеры
- [ ] Настроена автоматическая регистрация

### Фаза 6: Инфраструктура
- [ ] Создан GlobalExceptionHandler
- [ ] Настроена обработка исключений

### Фаза 7: Финальная очистка
- [ ] Удален временный код
- [ ] Оптимизирована структура
- [ ] Обновлена документация

---

## Потенциальные проблемы и решения

### Проблема 1: Циклические зависимости
**Решение:** Убедиться, что Domain не зависит от Application/Infrastructure.

### Проблема 2: Ошибки компиляции при миграции
**Решение:** Мигрировать по одной команде/запросу за раз, тестировать после каждого.

### Проблема 3: Проблемы с DI регистрацией
**Решение:** Проверить порядок регистрации декораторов, использовать Scrutor правильно.

### Проблема 4: Domain Events не вызываются
**Решение:** Убедиться, что сущности наследуются от Entity и вызывают Raise().

### Проблема 5: Endpoints не регистрируются
**Решение:** Проверить, что все Endpoints помечены как `internal sealed` и реализуют `IEndpoint`.

---

## Оценка времени

| Фаза | Время | Сложность |
|------|-------|-----------|
| Фаза 1: Фундамент | 1-2 дня | Средняя |
| Фаза 2: CQRS | 2-3 дня | Высокая |
| Фаза 3: Декораторы | 1-2 дня | Средняя |
| Фаза 4: Domain Events | 2-3 дня | Высокая |
| Фаза 5: API Layer | 2-3 дня | Средняя |
| Фаза 6: Инфраструктура | 1 день | Низкая |
| Фаза 7: Финальная очистка | 1 день | Низкая |
| **ИТОГО** | **10-15 дней** | |

---

## Заключение

Этот план миграции обеспечивает постепенный переход на новую архитектуру с возможностью тестирования на каждом этапе. Рекомендуется выполнять миграцию последовательно, не пропуская фазы, и тестировать после каждого шага.
