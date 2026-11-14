# ADR-0001: Project location and M0 scope

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

The repository must stay separate from employer Intellify workspaces and run locally with Docker on a developer machine.

## Decision

Host the codebase at `C:\Users\rmalik\TriageIQ` (alongside other personal projects). For M0, ship layered solution scaffolding, SQL Server and Azure Service Bus emulator in Compose, API `/health/live` and `/health/ready`, CI build/test/gitleaks, and `.env.example`—without ticket or messaging features yet.

## Consequences

Later milestones build on this layout without reshuffling folders. Health checks treat SQL and the Service Bus emulator as optional in the `Testing` environment so CI stays green without Docker.