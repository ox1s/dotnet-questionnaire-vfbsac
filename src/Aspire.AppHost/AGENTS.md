<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Aspire.AppHost

## Purpose
The .NET Aspire orchestrator entry point for the active stack — the executable project that boots the whole app locally with one command (`dotnet run` from this project, or via `aspire.config.json` at the repo root). It stands up a Postgres container with pgAdmin, the `Web.Api` project wired to that database, the `Web.Client` Vite frontend, and a public dev tunnel exposing both. This is the intended way to run the active stack end-to-end during development; there is no separate docker-compose for it.

## Key Files
| File | Description |
|------|-------------|
| `Program.cs` | Top-level-statements entry point. Builds the `DistributedApplicationBuilder`, then: (1) `AddPostgres("postgres")` on host port `5435` with pgAdmin, a persistent data volume, and `ContainerLifetime.Persistent` (survives `dotnet run` restarts); (2) `AddDatabase("questionnaire-vfbsac")` on that server; (3) `AddProject<Projects.Web_Api>("web-api")` wired to the database via `ConnectionStrings__Database` env var + `WithReference`/`WaitFor`; (4) `AddViteApp("frontend", "../Web.Client")` with external HTTP endpoints; (5) `AddDevTunnel("public-api")` referencing both backend and frontend with anonymous access. |
| `Aspire.AppHost.csproj` | Uses the `Aspire.AppHost.Sdk`, `OutputType=Exe`. References packages `Aspire.Hosting.AppHost`, `Aspire.Hosting.DevTunnels`, `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.JavaScript`, `MessagePack`, plus a `ProjectReference` to `Web.Api`. |

## For AI Agents
### Working In This Directory
- `obj/` contains build-generated files — `*.GlobalUsings.g.cs`, `*.AssemblyInfo.cs`, and the `Projects.*ProjectMetadata.g.cs` files under `obj/**/Aspire/references/` (these are what makes `Projects.Web_Api` resolve as a strongly-typed reference in `Program.cs`). Never hand-edit anything under `obj/` — it regenerates on every build from the `ProjectReference`s in the `.csproj`.
- The Postgres port (`5435`), database name (`questionnaire-vfbsac`), and container lifetime (`Persistent`, meaning the container/volume survive between `dotnet run` sessions) are all defined here — this is the single source of truth for local dev connection details for the active stack.
- If a new active-stack project needs to be part of the orchestrated run (e.g. a background worker), add it here with `builder.AddProject<Projects.X>(...)` and reference the project from `Aspire.AppHost.csproj`; the resource variable name becomes its identity in the Aspire dashboard.
- The dev tunnel (`public-api`) exposes both `backend` and `frontend` publicly with anonymous access — be deliberate before adding more resources to it, since anything referenced there becomes externally reachable during local dev.

## Dependencies
### Internal
`ProjectReference` to `../Web.Api/Web.Api.csproj` (referenced via the generated `Projects.Web_Api` type). Also implicitly orchestrates `../Web.Client` (Vite app, not a `ProjectReference` — pointed to by relative path string `"../Web.Client"`).

### External
`Aspire.Hosting.AppHost`, `Aspire.Hosting.DevTunnels`, `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.JavaScript`, `MessagePack` (NuGet packages, versions pinned centrally in `Directory.Packages.props`). Requires Docker (or another container runtime) running locally for the Postgres/pgAdmin containers.

<!-- MANUAL: -->
