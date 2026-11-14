# ADR-0003: M1 auth, JWT, and ticket persistence

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

Milestone M1 requires user accounts, JWT role enforcement, and ticket ingestion with idempotency before messaging and AI triage (M2+).

## Decision

Use ASP.NET Core JWT bearer authentication with roles in claims, `PasswordHasher<User>` for credentials, EF Core SQL Server in production, and SQLite in the `Testing` environment for WebApplicationFactory API tests. Idempotency is enforced with a unique index on `IdempotencyKey` and `GetOrCreate` semantics on duplicate keys.

## Consequences

The API auto-applies EF migrations on startup when a connection string is configured. Admin role changes are exposed at `PUT /api/users/{id}/role`. Outbox messaging remains deferred to M2.