<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Aspire.ServiceDefaults

## Purpose
Shared "service defaults" extension methods for .NET Aspire — the standard Aspire template project that wires up OpenTelemetry, health checks, service discovery, and HTTP resilience with one call. `Web.Api` calls `builder.AddServiceDefaults()` early in its `Program.cs` to pick all of this up; this project itself has no entry point and isn't runnable on its own.

## Key Files
| File | Description |
|------|-------------|
| `Extensions.cs` | Static class `Extensions` in namespace `Microsoft.Extensions.Hosting` (deliberately, so `AddServiceDefaults()` is available on any `IHostApplicationBuilder` without an extra `using`). Four public extension methods: `AddServiceDefaults` (calls the other three plus adds service discovery + a standard resilience handler to all `HttpClient`s via `ConfigureHttpClientDefaults`), `ConfigureOpenTelemetry` (logging with formatted messages/scopes; metrics via ASP.NET Core/HttpClient/Runtime/Npgsql instrumentation; tracing via ASP.NET Core/HttpClient/EF Core/Npgsql instrumentation), `AddDefaultHealthChecks` (registers a trivial always-healthy `"self"` check tagged `"live"`), and `MapDefaultEndpoints` (maps `/alive` — filtered to checks tagged `"live"` — but only when `app.Environment.IsDevelopment()`; the `/health` endpoint is commented out with a note about security implications of exposing it outside dev). A private `AddOpenTelemetryExporters` enables the OTLP exporter only if `OTEL_EXPORTER_OTLP_ENDPOINT` is configured. |

## For AI Agents
### Working In This Directory
- This is the stock Aspire "ServiceDefaults" template, largely unmodified — the file still carries the original template comments (e.g. "This project should be referenced by each service project in your solution") and several commented-out blocks (gRPC instrumentation, Azure Monitor exporter, restricting service-discovery schemes, the `/health` endpoint). Uncomment/extend rather than rewrite from scratch if new defaults are needed, to stay consistent with the Aspire template conventions this project follows.
- `<IsAspireSharedProject>true</IsAspireSharedProject>` in the `.csproj` marks this as an Aspire shared project — that's what makes it excluded from being an independently orchestrated Aspire resource.
- Only `Web.Api` currently calls `AddServiceDefaults()`. If another runnable project (e.g. a future worker service) joins the active stack, it should reference this project and call `AddServiceDefaults()`/`MapDefaultEndpoints()` the same way, rather than duplicating telemetry/health-check setup.
- `/alive` is intentionally dev-only; don't casually uncomment `/health` in non-development environments without considering the security note already in the comment (unauthenticated health endpoints can leak infra details).
- `obj/` contains build-generated files (`*.GlobalUsings.g.cs`, `*.AssemblyInfo.cs`) — never hand-edit, they regenerate on build.

## Dependencies
### Internal
None. Referenced by `Web.Api` (`ProjectReference`), not the other way around.

### External
`Microsoft.AspNetCore.App` (`FrameworkReference`), `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`, `Npgsql.OpenTelemetry`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime` (versions pinned centrally in `Directory.Packages.props`).

<!-- MANUAL: -->
