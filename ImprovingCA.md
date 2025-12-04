Of course. As the technical lead for this project, I've compiled a comprehensive refactoring guide to elevate the "Questionnaire" application to a more robust, maintainable, and testable state, mirroring the best practices from the "GymManagement" reference architecture.

Here is the complete plan in a single Markdown file. You can save this as `CleanArchitectureImprove.md` and use it as your step-by-step guide.

***

# Master Plan: Evolving the Questionnaire Application

This document outlines the steps to refactor the "Questionnaire" application, incorporating advanced architectural patterns like Domain-Driven Design (DDD), MediatR Pipelines for cross-cutting concerns, and a robust, multi-layered testing strategy. The goal is to enhance maintainability, scalability, and reliability.

## Phase 1: Strengthening the Domain Layer

The core of this refactoring is to make our domain model "richer" by encapsulating business logic within the entities themselves, rather than having it scattered in the application layer.

### Step 1.1: Create Base Classes for Entities and Domain Events

These will provide a foundation for tracking changes and dispatching events.

**1. Create `IDomainEvent.cs`**
This marker interface will identify our domain events.

**File:** `src/Questionnaire.Domain/Common/IDomainEvent.cs`
```csharp
using MediatR;

namespace Questionnaire.Domain.Common;

public interface IDomainEvent : INotification
{
}
```

**2. Create the `Entity` Base Class**
This class will manage a list of domain events for each entity instance.

**File:** `src/Questionnaire.Domain/Common/Entity.cs`
```csharp
namespace Questionnaire.Domain.Common;

public abstract class Entity
{
    public int Id { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Entity(int id)
    {
        Id = id;
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    // Add a parameterless constructor for EF Core
    protected Entity() { }
}
```

**3. Update Domain Entities to Inherit from `Entity`**
Modify `Form.cs`, `Question.cs`, `Answer.cs`, etc., to inherit from this new base class.

**Example: `Form.cs`**
```csharp
// src/Questionnaire.Domain/Entities/Form.cs
using Questionnaire.Domain.Common; // Add this using

public class Form : Entity // Inherit from Entity
{
    // The 'Id' property is now inherited. You can remove the existing one.
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FormQuestion> FormQuestions { get; set; } = new List<FormQuestion>();
    public ICollection<FormRole> FormRoles { get; set; } = new List<FormRole>();
    
    // Add a parameterless constructor for EF Core if it's not already there
    private Form() : base(0) { }

    public Form(string name, bool isActive = true) : base(0) // Assuming Id is DB-generated
    {
        Name = name;
        IsActive = isActive;
    }
}
```
*Apply this change to all other primary entities like `Question`, `Answer`, `User`, etc.*

### Step 1.2: Encapsulate Business Logic in Entities

Move logic from Application handlers into the domain models.

**1. Define Domain-Specific Errors**
Create a dedicated file for errors related to a specific entity.

**File:** `src/Questionnaire.Domain/Forms/FormErrors.cs`
```csharp
using ErrorOr;

namespace Questionnaire.Domain.Forms;

public static class FormErrors
{
    public static readonly Error QuestionAlreadyExists = Error.Conflict(
        code: "Form.QuestionAlreadyExists",
        description: "This question is already in the form.");
        
    public static readonly Error QuestionNotFound = Error.NotFound(
        code: "Form.QuestionNotFound",
        description: "This question is not found in the form.");
}
```

**2. Refactor `Form.cs` to Manage its Questions**
Make the `FormQuestions` collection private and expose methods to manipulate it.

**File:** `src/Questionnaire.Domain/Entities/Form.cs`
```csharp
using ErrorOr;
using Questionnaire.Domain.Common;
using Questionnaire.Domain.Forms; // Add this

public class Form : Entity
{
    private readonly List<FormQuestion> _formQuestions = new();
    
    // ... other properties
    
    public IReadOnlyList<FormQuestion> FormQuestions => _formQuestions.ToList();
    public ICollection<FormRole> FormRoles { get; private set; } = new List<FormRole>();

    // Constructor...
    private Form() : base(0) { }

    public Form(string name, bool isActive = true) : base(0)
    {
        Name = name;
        IsActive = isActive;
    }

    public ErrorOr<Success> AddQuestion(Question question, int order)
    {
        if (_formQuestions.Any(fq => fq.QuestionId == question.Id))
        {
            return FormErrors.QuestionAlreadyExists;
        }

        _formQuestions.Add(new FormQuestion
        {
            Form = this,
            Question = question,
            Order = order
        });

        return Result.Success;
    }

    public ErrorOr<Success> RemoveQuestion(int questionId)
    {
        var formQuestion = _formQuestions.FirstOrDefault(fq => fq.QuestionId == questionId);

        if (formQuestion is null)
        {
            return FormErrors.QuestionNotFound;
        }

        _formQuestions.Remove(formQuestion);
        
        return Result.Success;
    }
}
```

**3. Refactor `AddQuestionToFormCommandHandler.cs`**
The handler now becomes a simple orchestrator.

**File:** `src/Questionnaire.Application/Forms/Commands/AddQuestion/AddQuestionToFormCommandHandler.cs`
```csharp
// ...
public class AddQuestionToFormCommandHandler : IRequestHandler<AddQuestionToFormCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork; // We will add this in Phase 3

    public AddQuestionToFormCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(AddQuestionToFormCommand command, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.FormQuestions) // Eager load the collection
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);
            
        if (form is null)
        {
            return Error.NotFound(description: "Form not found.");
        }

        var question = await _context.Questions.FindAsync(command.QuestionId);
        if (question is null)
        {
            return Error.NotFound(description: "Question not found.");
        }

        var result = form.AddQuestion(question, command.Order);

        if (result.IsError)
        {
            return result.Errors;
        }
        
        // No need to add to _context.FormQuestions directly.
        // EF Core's change tracking will handle it.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
```
*Note: This refactoring assumes you will also implement `IUnitOfWork` as described in Phase 3. For now, you can keep using `_context.SaveChangesAsync()`.*

### Step 1.3: Implement Domain Events

**1. Define a Domain Event**
Let's create an event for when a form is deleted. This could be used later to clean up related data (like answers) in a decoupled way.

**File:** `src/Questionnaire.Domain/Forms/Events/FormDeletedEvent.cs`
```csharp
using Questionnaire.Domain.Common;

namespace Questionnaire.Domain.Forms.Events;

public record FormDeletedEvent(int FormId) : IDomainEvent;
```

**2. Raise the Event from the Entity**
Modify the `DeleteFormCommandHandler` to use a method on the `Form` entity, which in turn raises the event.

**File:** `src/Questionnaire.Domain/Entities/Form.cs`
```csharp
// Add this method to the Form class
public void MarkAsDeleted()
{
    AddDomainEvent(new FormDeletedEvent(Id));
}
```

**File:** `src/Questionnaire.Application/Forms/Commands/Delete/DeleteFormCommandHandler.cs`
```csharp
// ...
public async Task<ErrorOr<Success>> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
{
    var form = await _context.Forms.FindAsync(request.Id);
    if (form is null)
    {
        return Error.NotFound("Form not found.");
    }

    form.MarkAsDeleted(); // Raise the event

    _context.Forms.Remove(form);
    await _unitOfWork.SaveChangesAsync(cancellationToken); // Using Unit of Work

    return Result.Success;
}
```

**3. Create an Event Handler**
This handler will listen for the `FormDeletedEvent` and perform some action, like logging or deleting related answers.

**File:** `src/Questionnaire.Application/Forms/Events/FormDeletedEventHandler.cs`
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms.Events;

namespace Questionnaire.Application.Forms.Events;

public class FormDeletedEventHandler : INotificationHandler<FormDeletedEvent>
{
    private readonly ILogger<FormDeletedEventHandler> _logger;
    // You could inject IApplicationDbContext here to delete related answers, for example.

    public FormDeletedEventHandler(ILogger<FormDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FormDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Form with ID {FormId} was deleted.", notification.FormId);
        // Add logic here to handle the deletion, e.g., deleting associated answers.
        return Task.CompletedTask;
    }
}
```

---

## Phase 2: Refine Application & Infrastructure Layers

### Step 2.1: Implement MediatR Pipeline Behaviors

**1. Create `ValidationBehavior.cs`**
This will intercept all MediatR requests and run `FluentValidation` validators if they exist.

**File:** `src/Questionnaire.Application/Common/Behaviors/ValidationBehavior.cs`
```csharp
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Questionnaire.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
{
    private readonly IValidator<TRequest>? _validator = validator;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validator is null)
        {
            return await next();
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(error => Error.Validation(
                code: error.PropertyName,
                description: error.ErrorMessage));

        return (dynamic)errors;
    }
}
```

**2. Register the Behavior**
Update the `AddApplication` extension method.

**File:** `src/Questionnaire.Application/DependencyInjection.cs`
```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Questionnaire.Application.Common.Behaviors; // Add this

namespace Questionnaire.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register the pipeline behavior
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

### Step 2.2: Implement `IUnitOfWork` and Event Dispatching

This pattern ensures that domain events are only dispatched *after* the database transaction is successfully committed.

**1. Define `IUnitOfWork.cs`**
This interface already exists in your project, but ensure it's in the correct location.

**File:** `src/Questionnaire.Application/Common/Interfaces/IUnitOfWork.cs`
```csharp
namespace Questionnaire.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**2. Refactor `ApplicationDbContext`**
Modify the DbContext to handle domain events. This requires injecting `IPublisher` from MediatR.

**File:** `src/Questionnaire.Infrastructure/Persistence/ApplicationDbContext.cs`
```csharp
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Common;
// ... other usings

namespace Questionnaire.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IPublisher publisher,
        IHttpContextAccessor httpContextAccessor) // Inject HttpContextAccessor
        : base(options)
    {
        _publisher = publisher;
        _httpContextAccessor = httpContextAccessor;
    }

    // ... DbSets ...

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity.GetDomainEvents())
            .SelectMany(x => x)
            .ToList();
        
        // Clear events before saving to prevent re-publishing on failure
        ChangeTracker.Entries<Entity>()
            .ToList()
            .ForEach(entry => entry.Entity.ClearDomainEvents());

        // For HTTP requests, we delay publishing until after the response is sent.
        if (_httpContextAccessor.HttpContext is not null)
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
        }
        else // For background jobs or tests, publish immediately
        {
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
    {
        var queue = _httpContextAccessor.HttpContext!.Items
            .TryGetValue("DomainEventsQueue", out var value) && value is Queue<IDomainEvent> q
                ? q
                : new Queue<IDomainEvent>();

        domainEvents.ForEach(queue.Enqueue);
        _httpContextAccessor.HttpContext!.Items["DomainEventsQueue"] = queue;
    }

    // ... OnModelCreating ...
}
```

**3. Create and Register Eventual Consistency Middleware**
This middleware ensures events are published only after the HTTP response is successfully sent, confirming the transaction commit.

**File:** `src/Questionnaire.Infrastructure/Common/Middleware/EventualConsistencyMiddleware.cs`
```csharp
using MediatR;
using Microsoft.AspNetCore.Http;
using Questionnaire.Domain.Common;
using Questionnaire.Infrastructure.Persistence;

namespace Questionnaire.Infrastructure.Common.Middleware;

public class EventualConsistencyMiddleware
{
    private readonly RequestDelegate _next;

    public EventualConsistencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPublisher publisher, ApplicationDbContext dbContext)
    {
        // Begin transaction before the request pipeline
        var transaction = await dbContext.Database.BeginTransactionAsync();
        context.Response.OnCompleted(async () =>
        {
            try
            {
                if (context.Items.TryGetValue("DomainEventsQueue", out var value) &&
                    value is Queue<IDomainEvent> domainEventsQueue)
                {
                    while (domainEventsQueue.TryDequeue(out var domainEvent))
                    {
                        await publisher.Publish(domainEvent);
                    }
                }
                // Commit transaction after events are queued
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Handle potential errors during event publishing
                // Log the error, but don't re-throw as the response is already sent
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        });

        await _next(context);
    }
}
```

**4. Register Middleware in `Program.cs`**
Create a new extension method for this.

**File:** `src/Questionnaire.Infrastructure/RequestPipeline.cs`
```csharp
using Microsoft.AspNetCore.Builder;
using Questionnaire.Infrastructure.Common.Middleware;

namespace Questionnaire.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder AddInfrastructureMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<EventualConsistencyMiddleware>();
        return builder;
    }
}
```

**File:** `src/Questionnaire.Api/Program.cs`
```csharp
// ...
var app = builder.Build();
{
    app.UseExceptionHandler();
    app.AddInfrastructureMiddleware(); // Add this line
    // ...
}
```

**5. Update `DependencyInjection.cs` in Infrastructure**
Ensure `IUnitOfWork` is registered correctly.

**File:** `src/Questionnaire.Infrastructure/DependencyInjection.cs`
```csharp
// ...
services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>()); // Add this line
// ...
```

---

## Phase 4: Implement a Comprehensive Testing Strategy

Create new test projects for each layer.

1.  **Domain Unit Tests:** Test business logic in isolation.
    *   Create a new xUnit project: `Questionnaire.Domain.UnitTests`.
    *   Add a project reference to `Questionnaire.Domain`.
    *   Add NuGet packages: `FluentAssertions`.
    *   **Example Test:**
        ```csharp
        // tests/Questionnaire.Domain.UnitTests/Forms/FormTests.cs
        public class FormTests
        {
            [Fact]
            public void AddQuestion_WhenQuestionAlreadyExists_ShouldReturnError()
            {
                // Arrange
                var form = new Form("Test Form");
                var question = new Question("Test Question", QuestionType.Text);
                form.AddQuestion(question, 1);

                // Act
                var result = form.AddQuestion(question, 2);

                // Assert
                result.IsError.Should().BeTrue();
                result.FirstError.Should().Be(FormErrors.QuestionAlreadyExists);
            }
        }
        ```

2.  **Application Subcutaneous Tests:** Test the application layer's logic, including database interactions (mocked or in-memory).
    *   Create a new xUnit project: `Questionnaire.Application.SubcutaneousTests`.
    *   Add project references to `Questionnaire.Application` and `Questionnaire.Infrastructure`.
    *   Add NuGet packages: `FluentAssertions`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Mvc.Testing`.
    *   Implement `MediatorFactory.cs` and `SqliteTestDatabase.cs` similar to the `GymManagement` example to provide a clean, isolated environment for each test.
    *   **Example Test:**
        ```csharp
        // tests/Questionnaire.Application.SubcutaneousTests/Forms/Commands/CreateFormTests.cs
        public class CreateFormTests(MediatorFactory factory)
        {
            private readonly IMediator _mediator = factory.CreateMediator();

            [Fact]
            public async Task CreateForm_WhenNameIsValid_ShouldCreateAndReturnForm()
            {
                // Arrange
                var command = new CreateFormCommand("My Awesome Form");

                // Act
                var result = await _mediator.Send(command);

                // Assert
                result.IsError.Should().BeFalse();
                result.Value.Name.Should().Be("My Awesome Form");
            }
        }
        ```

3.  **API Integration Tests:** Test the full request/response cycle.
    *   Create a new xUnit project: `Questionnaire.Api.IntegrationTests`.
    *   Add a project reference to `Questionnaire.Api`.
    *   Add NuGet packages: `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`.
    *   Implement `QuestionnaireApiFactory.cs` similar to the `GymManagement` example.
    *   **Example Test:**
        ```csharp
        // tests/Questionnaire.Api.IntegrationTests/Controllers/FormsControllerTests.cs
        public class FormsControllerTests(QuestionnaireApiFactory factory)
        {
            private readonly HttpClient _client = factory.CreateClient();

            [Fact]
            public async Task CreateForm_WithValidRequest_ShouldReturnCreated()
            {
                // Arrange
                var request = new CreateFormRequest("Integration Test Form");

                // Act
                var response = await _client.PostAsJsonAsync("/forms", request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                var formResponse = await response.Content.ReadFromJsonAsync<FormResponse>();
                formResponse.Should().NotBeNull();
                formResponse.Name.Should().Be(request.Name);
                response.Headers.Location.Should().NotBeNull();
            }
        }
        ```

This structured approach will transform your project into a more robust, maintainable, and testable application, aligning it with modern best practices in software architecture. Let me know if you'd like to dive deeper into any of these steps