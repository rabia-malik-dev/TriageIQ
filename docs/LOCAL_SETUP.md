# TriageIQ — local setup and manual testing

Use this guide on a Windows machine with Docker Desktop. The project lives at `C:\Users\rmalik\TriageIQ` (separate from Intellify and AI Incident).

## 1. Prerequisites

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) and ensure it is **running**.
2. Install [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (optional, for `dotnet test` without Docker rebuild).
3. Clone or open the repo: `https://github.com/rabia-malik-dev/TriageIQ`

## 2. Configure environment

```powershell
cd C:\Users\rmalik\TriageIQ
copy .env.example .env
```

Edit `.env` and confirm at least:

| Variable | Purpose |
|----------|---------|
| `JWT_SIGNING_KEY` | **32+ characters** — signs login tokens |
| `SEED_ADMIN_EMAIL` | Dev-only first admin (e.g. `admin@local.test`) |
| `SEED_ADMIN_PASSWORD` | Dev-only seeded admin password (any non-empty value) |
| `SQLSERVER_SA_PASSWORD` | SQL Server SA password (match compose) |
| `ACCEPT_EULA` | `Y` |

Never commit `.env`.

## 3. Build and start (Docker)

```powershell
docker compose up --build -d
```

Wait until containers are running in Docker Desktop:

- `triageiq-sqlserver` (healthy)
- `triageiq-servicebus-emulator`
- `triageiq-api` (port **8080**)
- `triageiq-worker`

Check status:

```powershell
docker compose ps
```

## 4. Verify health (browser)

Open:

| URL | Expected |
|-----|----------|
| http://localhost:8080/health/live | `"status":"Healthy"` |
| http://localhost:8080/health/ready | SQL + Service Bus healthy |

## 5. Swagger UI (recommended for M1)

In **Development** (Docker uses this), open:

**http://localhost:8080/swagger**

Use **Authorize** (top right):

1. Call **POST /api/auth/login** with seeded admin:
   ```json
   {
     "email": "admin@local.test",
     "password": "ChangeMe-Admin-123!"
   }
   ```
   (Use your `.env` values if different.)

2. Copy `accessToken` from the response.

3. Click **Authorize**, enter: `Bearer <paste token>` (include the word `Bearer` and a space).

4. Try **GET /api/tickets**, **POST /api/auth/register**, etc.

Raw OpenAPI JSON (no UI): http://localhost:8080/openapi/v1.json

## 6. Typical M1 test flow in Swagger

1. **POST /api/auth/register** — create a user (default role **Viewer**).
2. **POST /api/auth/login** as **admin@local.test** — get admin token; Authorize.
3. **PUT /api/users/{userId}/role** — set the new user to **Reviewer** (use `userId` from JWT `sub` after they register/login).
4. Login as the reviewer; Authorize with their token.
5. **POST /api/tickets** — add header **Idempotency-Key** (Swagger: use "Try it out" and add the header) with a new UUID each time.
6. **GET /api/tickets** — list tickets.

Repeat **POST /api/tickets** with the **same** Idempotency-Key — response must return the **same** ticket id.

## 7. Logs and restart

```powershell
docker compose logs -f api
docker compose up --build -d api
docker compose down
```

## 8. Run automated tests (no Docker required)

```powershell
dotnet test TriageIQ.slnx --configuration Release
```

## 9. M2 — verify messaging (Docker)

After creating a ticket in Swagger:

1. **SQL — outbox published:**
   ```sql
   USE TriageIQ;
   SELECT Id, TicketId, Status, PublishedAt FROM OutboxMessages ORDER BY CreatedAt DESC;
   ```
   `Status` should become `Published` within a few seconds.

2. **SQL — worker consumed message:**
   ```sql
   SELECT Id, Subject, Status FROM Tickets ORDER BY CreatedAt DESC;
   ```
   `Status` should change from `Received` to `QueuedForTriage`.

3. **Worker logs:**
   ```powershell
   docker compose logs -f worker
   ```
   Look for: `Ticket {id} queued for triage`.

4. **DLQ (manual / advanced):** emulator queue `tickets.incoming` uses `MaxDeliveryCount: 5` in `deploy/servicebus/Config.json`. Repeated handler failures abandon the message until Service Bus dead-letters it.

## 10. M3 — verify LLM triage (Docker)

After M2 flow completes (ticket reaches worker):

1. **Swagger:** **GET /api/tickets/{ticketId}/analysis** (same JWT as ticket create).
2. Expect JSON with `category`, `priority`, `summary`, `confidence`, and provider **`local`** (development stub—no cloud API).
3. **SQL:**
   ```sql
   SELECT Id, Status, BodyRedacted FROM Tickets WHERE Id = '<ticket-id>';
   SELECT Category, Confidence, RoutingDecision FROM TicketAnalyses;
   SELECT Provider, Model, InputTokens, OutputTokens FROM LlmCalls;
   ```
4. Subject contains **payment** → local development provider typically returns `billing` + higher confidence (`AutoApproved` if ≥ 0.85).

Default: `LLM_PROVIDER=local` in `.env`. No external API key required for local Docker.

## 11. M4 — guardrails

After triage runs:

1. **Swagger:** **GET /api/tickets/{ticketId}/analysis** includes `reviewReason` when routing is not a clean auto-approve (e.g. `low_confidence`, `cost_budget_exceeded`).
2. **Injection test:** create a ticket whose body contains `ignore previous instructions` — worker should set status **NeedsReview**, `reviewReason` = `prompt_injection`, and `LlmCalls.Provider` = `guardrail` (no LLM call).
3. **Config:** `Triage__MaxEstimatedCostUsd` (default `0.25`) alongside `Triage__ConfidenceThreshold` in Docker / `appsettings`.

## 12. What is not available yet

- **M5+:** Angular review UI, real Anthropic provider, eval harness.
- Swagger is **not** enabled in production; only Development/Docker local stack.

See [SPEC.md](./SPEC.md) for the full milestone plan.