<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Web.Api

## Purpose
The composition root and active ASP.NET Core host for the questionnaire platform. Booted by `src/Aspire.AppHost` alongside a Postgres database and the `src/Web.Client` Vite dev server. `Program.cs` wires `AddApplication()` + `AddPresentation()` + `AddInfrastructure()`, registers Serilog request-context logging, API versioning, Swagger/OpenAPI with JWT bearer auth, health checks, and — on every startup — runs EF Core migrations (`context.Database.MigrateAsync()`) and the `DbInitializer` seeder before `app.RunAsync()`. Despite the repo-wide convention of "prefer controllers over minimal APIs" (`.cursor/rules/dotnet-rules.mdc`), every actual feature endpoint in this project is a minimal-API route registered through a small `IEndpoint`-per-class pattern (see `Endpoints/AGENTS.md`) — `AddControllers()`/`MapControllers()` are called in `DependencyInjection.cs`/`Program.cs` but no `Controllers/` directory exists, so that call currently maps zero controllers. Treat it as unused scaffolding, not the real routing mechanism.

## Key Files
| File | Description |
|------|-------------|
| `Program.cs` | Entry point / composition root. Order: `AddServiceDefaults()` (Aspire) → `AddSwaggerGenWithAuth()` → `AddApplication()` + `AddPresentation()` + `AddInfrastructure(config)` → `AddApiVersioning()`/`AddApiExplorer()` → `AddEndpoints(assembly)` (reflection-scans for `IEndpoint`) → build app → `MapDefaultEndpoints()` → `MapEndpoints()` → Swagger UI (Development only) → `MapHealthChecks("health", ...)` → `UseRequestContextLogging()` → `UseExceptionHandler()` → `UseAuthentication()`/`UseAuthorization()` → `MapControllers()` (no-op, no controllers defined) → EF Core `MigrateAsync()` + `DbInitializer.InitializeAsync()` in a scope → `RunAsync()`. Declares `partial class Program` in `namespace Web.Api` at the bottom for integration-test `WebApplicationFactory<Program>` access. |
| `DependencyInjection.cs` | `AddPresentation()` extension: `AddEndpointsApiExplorer()`, `AddSwaggerGen()`, `AddControllers()`, configures `JsonStringEnumConverter` for `JsonOptions` (enums serialize as strings), registers `GlobalExceptionHandler` via `AddExceptionHandler<T>()` + `AddProblemDetails()`. |
| `Web.Api.csproj` | `Microsoft.NET.Sdk.Web` project. References `Aspire.ServiceDefaults`, `Infrastructure`, `SharedKernel`, `Application` (not `Domain` directly — reached transitively). Packages: `Asp.Versioning.Mvc(.ApiExplorer)`, `AspNetCore.HealthChecks.UI.Client`, `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore`, `Microsoft.OpenApi`, EF Core Tools. |
| `Properties/launchSettings.json` | `http`/`https`/`IIS Express`/`Docker` profiles, all launching `swagger` on startup; default ports 5000 (HTTP) / 5001 (HTTPS). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Endpoints/` | All minimal-API route definitions, one `IEndpoint` implementation per operation, grouped into 10 feature folders (see `Endpoints/AGENTS.md` for the full route table) |
| `Extensions/` | DI/pipeline registration extension methods (see below) |
| `Infrastructure/` | Presentation-layer helpers: `CustomResults.Problem` and `GlobalExceptionHandler` (see below) |
| `Middleware/` | Custom ASP.NET Core middleware: `RequestContextLoggingMiddleware` (see below) |
| `Properties/` | `launchSettings.json` only |

### `Extensions/` (short reference — the main DI/pipeline registration surface alongside `Program.cs`)
| File | Description |
|------|-------------|
| `EndpointExtensions.cs` | `AddEndpoints(Assembly)` reflection-scans the assembly for non-abstract types assignable to `IEndpoint` and registers each as a transient `IEndpoint` via `TryAddEnumerable`. `MapEndpoints(WebApplication, RouteGroupBuilder?)` resolves all registered `IEndpoint`s from DI and calls `MapEndpoint` on each (against a `RouteGroupBuilder` if one is supplied, otherwise directly against the app — currently `Program.cs` always calls it with no group, so no shared route prefix/versioning is applied despite API versioning being configured). `HasPermission(RouteHandlerBuilder, string permission)` is a thin wrapper over `RequireAuthorization(permission)`, used pervasively by endpoints instead of calling `RequireAuthorization` directly. |
| `ServiceCollectionExtensions.cs` | `internal` `AddSwaggerGenWithAuth()` configures Swashbuckle: custom schema IDs (`+` replaced with `-` for nested/record types), adds a `Bearer` JWT security scheme, and a global security requirement so Swagger UI shows the "Authorize" button. |
| `ApplicationBuilderExtensions.cs` | `UseSwaggerWithUi()` — thin wrapper calling `UseSwagger()` + `UseSwaggerUI()`. Only invoked when `app.Environment.IsDevelopment()`. |
| `MiddlewareExtensions.cs` | `UseRequestContextLogging()` — registers `RequestContextLoggingMiddleware`. |
| `ResultExtensions.cs` | `Match<TOut>` / `Match<TIn, TOut>` extensions on `SharedKernel.Result`/`Result<TIn>` — the standard way endpoints convert a `Result` into an `IResult` (`result.Match(Results.Ok, CustomResults.Problem)` is the dominant idiom across `Endpoints/`). |

### `Infrastructure/`
| File | Description |
|------|-------------|
| `CustomResults.cs` | `CustomResults.Problem(Result result)` maps a failed `SharedKernel.Result` to an RFC 7807 `ProblemDetails` response via `Results.Problem(...)`. Maps `ErrorType.Validation`/`Problem`/`NotFound`/`Conflict` to 400/400/404/409 respectively (anything else, including the default case, falls back to 500 "Server failure" with a generic detail message). If the error is a `ValidationError`, attaches its `Errors` collection under an `"errors"` extension. Throws `InvalidOperationException` if called on a successful `Result` (programmer error). |
| `GlobalExceptionHandler.cs` | `internal sealed class GlobalExceptionHandler : IExceptionHandler`, registered via `AddExceptionHandler<T>()`/`UseExceptionHandler()`. Logs the exception at `LogError`, then writes a generic 500 `ProblemDetails` ("Server failure") — deliberately does not leak exception details to the client. This is the last-resort handler for *unexpected* exceptions; expected/business failures should flow through `Result`/`CustomResults.Problem` instead. |

### `Middleware/`
| File | Description |
|------|-------------|
| `RequestContextLoggingMiddleware.cs` | Non-primary-constructor style `class RequestContextLoggingMiddleware(RequestDelegate next, ILogger<...> logger)`. On every request: reads a `CorrelationId` request header (falls back to `HttpContext.TraceIdentifier` if absent), reads the current user's `ClaimTypes.NameIdentifier` claim (if authenticated) and tags the current `Activity` with `user.id`, then wraps `next.Invoke(context)` in a Serilog `logger.BeginScope(data)` so `CorrelationId`/`UserId` are attached to every log line for that request. |

## For AI Agents
### Working In This Directory
- New feature endpoints should follow the existing vertical-slice pattern: one `internal sealed class` per operation implementing `IEndpoint`, placed under `Endpoints/<FeatureGroup>/<Verb>.cs` (e.g. `Endpoints/Departments/Create.cs`). No manual registration is needed — `AddEndpoints(Assembly.GetExecutingAssembly())` in `Program.cs` picks up any `IEndpoint` implementation automatically via reflection.
- Endpoint bodies are thin: bind a request/command, call the matching `Application` layer `ICommandHandler<TCommand[, TResponse]>`/`IQueryHandler<TQuery, TResponse>`, then convert the `SharedKernel.Result` to an `IResult` via `.Match(Results.Ok, CustomResults.Problem)` (or `Results.NoContent`/`Results.File` for other verbs). Never put business logic in `Web.Api` — it belongs in `Application`/`Domain`.
- Authorization is applied per-route, not globally: `.RequireAuthorization()` for "any authenticated user", `.HasPermission(Permissions.X)` (defined in `src/SharedKernel/Permissions.cs`: `Admin`, `UsersAccess`, `DictionariesWrite`, `ReportsView`, `SubmitForms`) for permission-gated routes, and `.AllowAnonymous()` only on `Users/Login.cs`. Forgetting one of these on a new endpoint leaves it open to any authenticated caller by ASP.NET Core default — always add an explicit auth call.
- The `.HasPermission(...)` extension (`Extensions/EndpointExtensions.cs`) and `RequireAuthorization(Permissions.X)` are used interchangeably in the codebase (compare `Users/GetById.cs` vs `Submissions/GetList.cs`) — both resolve to the same policy-based check; prefer `.HasPermission(...)` for new code since it's the more common idiom.
- API versioning (`Asp.Versioning`) is registered in `Program.cs` but not actually applied to any route — no endpoint uses `WithApiVersionSet()`/a `v{version}` prefix, and `MapEndpoints()` is called with no `RouteGroupBuilder`. Don't assume routes are versioned; they currently are not.
- Route templates are flat, lower-case, unprefixed strings (e.g. `"users/login"`, `"departments/{departmentId:guid}"`, `"dictionaries/departments"`) — no leading `/api` or version segment. Match this convention for new routes.
- Response shapes flow straight from `Application`-layer query/command response records; request DTOs are usually private/nested `record Request(...)`/`record CreateXRequest(...)` types declared inside the endpoint class itself (see `Users/Login.cs`, `Disciplines/Update.cs`) rather than shared contract types.
- `WithTags(...)` groups routes in Swagger. Most groups use their folder name as the tag (`"Forms"`, `"Reports"`, `"Submissions"`, `"Teachers"`), but several dictionary-style groups (`Departments`, `Disciplines`, `Specialities`, `Specializations`) and even one `Teachers/Restore.cs` endpoint use the literal string `"Dictionaries"` or `Tags.Settings`/`Tags.Users` constants from `Endpoints/Tags.cs` — `Tags.cs` is incomplete (only defines `Users`, `Submissions`, `Forms`, `Departments`, `Teachers`, `Settings`) and not consistently used; check both before assuming a tag name.
- `AddControllers()`/`MapControllers()` exist in the pipeline but there is currently no `Controllers/` directory — don't be misled into thinking controller-based MVC endpoints are the primary mechanism here; they aren't, minimal `IEndpoint` classes are.
- A few files contain stray non-English (Russian) inline comments (e.g. `Extensions/ServiceCollectionExtensions.cs`, `Endpoints/Dictionaries/GetDisciplines.cs`) — leftover from AI-assisted edits; harmless, no action needed unless you're touching that exact line.

## Dependencies
### Internal
- `Application` (command/query handlers, abstractions like `ICommandHandler`/`IQueryHandler`/`IUserContext`)
- `Infrastructure` (`ApplicationDbContext`, `DbInitializer`, concrete auth/permission services registered via `AddInfrastructure`)
- `SharedKernel` (`Result`/`Error`/`Permissions`)
- `Aspire.ServiceDefaults` (`AddServiceDefaults()`/`MapDefaultEndpoints()` — health checks, telemetry, resilience)

### External
- ASP.NET Core minimal APIs, `Microsoft.EntityFrameworkCore` (migrations at startup)
- `Asp.Versioning.Mvc` / `Asp.Versioning.Mvc.ApiExplorer` (registered, not yet applied to routes)
- `Swashbuckle.AspNetCore` / `Microsoft.OpenApi` (Swagger/OpenAPI + JWT bearer scheme)
- `AspNetCore.HealthChecks.UI.Client` (health check UI response writer)
- Serilog (via `RequestContextLoggingMiddleware`'s `ILogger.BeginScope`)
- JWT bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer` — scheme referenced in Swagger security definition; actual `AddAuthentication`/`AddJwtBearer` registration lives in `Infrastructure`, not this project)

<!-- MANUAL: -->
