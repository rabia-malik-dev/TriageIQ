# Architecture

TriageIQ uses a layered backend under `src/`:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `TriageIQ.Domain` | Entities and value objects (empty in M0) |
| Application | `TriageIQ.Application` | Use cases, interfaces (`IClock` in M0) |
| Infrastructure | `TriageIQ.Infrastructure` | External systems, health checks, DI wiring |
| API | `TriageIQ.Api` | HTTP controllers, OpenAPI, Swagger UI (Development) |
| Worker | `TriageIQ.Worker` | Background processing (scaffold in M0) |
| Evals | `TriageIQ.Evals` | Evaluation CLI (scaffold; M6) |

## Local runtime (M0)

`docker compose up` starts:

- **sqlserver** — application SQL Server 2022
- **servicebus-sqledge** + **servicebus-emulator** — Azure Service Bus emulator with queue `tickets.incoming` in config
- **api** — ASP.NET Core API on port 8080
- **worker** — placeholder worker process

The API readiness probe checks SQL Server and the Service Bus emulator health URL when configured.

**M1:** JWT auth, users, tickets (EF migrations on startup). **M2:** transactional outbox, API publisher to `tickets.incoming`, worker consumer → `QueuedForTriage`. **M3:** fake `ILlmClient`, `TicketAnalyses` + `LlmCalls`, `GET .../analysis`. **Development:** Swagger at `/swagger` — see [LOCAL_SETUP.md](./LOCAL_SETUP.md).

Full message flow and components are defined in [SPEC.md](./SPEC.md).