# Architecture (M0)

TriageIQ uses a layered backend under `src/`:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `TriageIQ.Domain` | Entities and value objects (empty in M0) |
| Application | `TriageIQ.Application` | Use cases, interfaces (`IClock` in M0) |
| Infrastructure | `TriageIQ.Infrastructure` | External systems, health checks, DI wiring |
| API | `TriageIQ.Api` | HTTP controllers, OpenAPI |
| Worker | `TriageIQ.Worker` | Background processing (scaffold in M0) |
| Evals | `TriageIQ.Evals` | Evaluation CLI (scaffold; M6) |

## Local runtime (M0)

`docker compose up` starts:

- **sqlserver** — application SQL Server 2022
- **servicebus-sqledge** + **servicebus-emulator** — Azure Service Bus emulator with queue `tickets.incoming` in config
- **api** — ASP.NET Core API on port 8080
- **worker** — placeholder worker process

The API readiness probe checks SQL Server and the emulator health URL when configured.

Full message flow and components are defined in [SPEC.md](./SPEC.md).