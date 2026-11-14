# TriageIQ — Product specification (source of truth)

Cursor Build Prompt — AI Ticket Triage Reliability Platform · Nov 14, 2026 · @rabia

This file is the authoritative specification. If implementation and this document disagree, raise a spec change before coding.

---

## 1. Role and goal

Build **TriageIQ**: a production-style, event-driven platform that triages customer support tickets with an LLM, applies guardrails, sends uncertain cases to human review, and proves quality with automated evaluations, observability, and cost tracking.

Optimise for **correctness, tests, clarity**, and **one-command local setup** (`docker compose up`), not feature count.

**How to work:** One milestone at a time (section 10); stop for review at each end. No scope creep. Tests with every feature. Conventional Commits. ADRs in `docs/adr/` for significant decisions.

Publishing rules: section 11 (GitHub, commit identity). Private study guides: section 12 (M9).

---

## 2. Product scope

**In scope:** REST ticket ingestion (single/bulk, idempotent); async AI triage; guardrails; human review queue; prompt versioning; evaluation harness with CI gate; analytics and ops UI; JWT (Admin, Reviewer, Viewer); observability (logs, traces, metrics, Grafana).

**Out of scope:** Multi-tenancy, real email/helpdesk integrations, payments, fine-tuning, mobile apps, vector/RAG (stretch only).

**Data rule:** Synthetic tickets only. No real PII in repo or commits.

---

## 3. Technology stack

| Area | Choice |
|------|--------|
| Backend | Latest LTS .NET, nullable, warnings as errors; ASP.NET Core controllers; EF Core; FluentValidation; Polly; OpenAPI |
| Worker | .NET Worker Service (`BackgroundService`) |
| Messaging | Azure Service Bus `tickets.incoming` + DLQ; emulator in Docker; `IMessageBus` abstraction |
| LLM | Claude via `ILlmClient`; local development provider by default; config/env for keys and models |
| Database | SQL Server 2022 in Docker; EF migrations |
| Frontend | Angular (standalone, signals, strict TS); Chart.js; Material or Bootstrap |
| Auth | JWT; `PasswordHasher`; role-based auth |
| Observability | Serilog; OpenTelemetry; OTLP; Jaeger; Prometheus; Grafana (compose profile) |
| Testing | xUnit, FluentAssertions, NSubstitute, Testcontainers, WebApplicationFactory, Playwright |
| DevOps | Docker Compose; GitHub Actions; GHCR; Bicep (stretch) |

**Setup:** clone → copy `.env.example` → `.env` → `LLM_PROVIDER=local` (or cloud API key when enabled) → `docker compose up`.

---

## 4. Architecture and message flow

Layers: **Domain → Application → Infrastructure → Api / Worker / Evals**. Angular via Nginx.

1. `POST /api/tickets` + JWT + `Idempotency-Key` → save ticket `Received` + outbox (same transaction).
2. Outbox publisher → `TicketReceived` on `tickets.incoming`.
3. Worker: PII redact → prompt → `ILlmClient` (timeout, retry, circuit breaker).
4. Parse/validate JSON schema → guardrails → `AutoApproved` | `NeedsReview` | `Failed`.
5. Save analysis + `LlmCall` (tokens, cost, latency); update ticket status.
6. Retries via Service Bus; 5 failures → DLQ; Admin replay from ops.
7. Reviewers approve/edit/reject; edits feed evaluation dataset.
8. One **correlation id** across HTTP, message, LLM, logs.

---

## 5–7. Features, data model, AI

See repository `README.md` and milestones. Tables: `Users`, `Tickets`, `TicketAnalyses`, `LlmCalls`, `ReviewActions`, `PromptVersions`, `EvalRuns`, `OutboxMessages`, `AuditLogs`.

API surface under `/api` with roles as in build prompt (auth, tickets, reviews, prompts, evals, analytics, ops, health).

Guardrails: PII redaction, prompt-injection detection, schema validation (+ one repair retry), confidence ≥ 0.85 (configurable), cost budgets, never auto-send replies.

Evaluation: `eval/datasets/triage-v1.jsonl` (100 synthetic tickets); replay (CI) vs live; quality gate thresholds in build prompt.

---

## 8. Observability, testing, CI/CD

Structured logs (no PII/secrets); traces for HTTP, publish, consume, LLM, DB; metrics listed in build prompt; Grafana provisioned JSON.

Test pyramid: unit (guardrails), integration (Testcontainers, fake bus), API (WebApplicationFactory), Angular + Playwright E2E.

Workflows: `ci.yml`, `eval-live.yml`, `release.yml`, `deploy.yml` (stretch); Dependabot; PR template; CODEOWNERS; gitleaks in CI.

---

## 9. Repository structure

```
triageiq/
  src/          Domain, Application, Infrastructure, Api, Worker, Evals
  web/          Angular
  tests/        UnitTests, IntegrationTests, e2e/
  eval/datasets/
  deploy/       Bicep, Grafana, collector, service bus config
  docs/         SPEC.md, adr/, architecture.md
  .github/workflows/
  docker-compose.yml
  .env.example
  README.md
```

---

## 10. Milestones

| Milestone | Scope | Acceptance (summary) |
|-----------|--------|----------------------|
| **M0** | Layout, Compose (SQL + bus emulator), CI skeleton, health, `.env.example`, ADR template | `docker compose up` clean; CI green; `/health/ready` healthy |
| **M1** | Auth, JWT, tickets CRUD, idempotency | Role rules; duplicate idempotency key; API tests |
| **M2** | Outbox, worker, DLQ | End-to-end to worker; DLQ after 5 tries |
| **M3** | LLM client, prompts, schema | Analysis + `LlmCall` records |
| **M4** | Guardrails | Unit tests; review routing |
| **M5** | Angular review UI | Playwright flow |
| **M6** | Eval harness + CI gate | Regression fails CI |
| **M7** | OTel, Grafana, analytics, ops | Trace + replay |
| **M8** | Release docs, GHCR | 10-minute stranger onboarding |
| **M9** | Private study guides (PDF) | Section 12 |

**Done per milestone:** no warnings; tests pass; docs/ADRs updated; no secrets; reviewer sign-off.

**Stretch (after M8):** SQS adapter, RAG, Bicep deploy, CloudWatch export.

---

## 11. GitHub repository and commit identity

- Public repo **TriageIQ** under `rabia-malik-dev` (gh CLI; topics per build prompt).
- `main` branch; MIT License © Rabia Malik; `.gitignore`, `.gitattributes`, PR template, CODEOWNERS.
- Milestone branches → PRs (e.g. `feat/m0-foundation`).
- Local git: `user.name` = Rabia Malik; `user.email` = GitHub no-reply address (configure per repo).
- No `Co-authored-by:` or tool attribution; `.githooks/commit-msg` strips Co-authored lines; `core.hooksPath .githooks` (see `CONTRIBUTING.md`).
- Conventional Commits; gitleaks in CI; never commit `.env` or `study-guides/`.
- After M8: tag `v0.1.0`.

---

## 12. Private study guides (M9)

Personal PDFs from Markdown in `study/src/` → `study-guides/` via `python tools/study/build_pdfs.py`. Both paths **gitignored**. Practice-question format, difficulty mix, coverage map, and guide list are defined in the full build prompt (section 12 of the Cursor spec).

Learning log: append to `study/notes/` after each milestone (ignored).

---

*This document summarises the Cursor Build Prompt. For section 12 question-format detail and the full guide catalogue, refer to the same prompt in project history or expand this file when M9 starts.*