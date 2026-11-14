# ADR-0004: Swagger UI in Development

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

M1 endpoints are mostly POST and require JWT headers. Browser address bars only send GET requests, which confuses local testing.

## Decision

Add Swashbuckle Swagger UI at `/swagger` when `ASPNETCORE_ENVIRONMENT` is Development (including Docker Compose). Configure JWT bearer security so tokens from login can authorize ticket calls. Keep `/openapi/v1.json` for the built-in OpenAPI endpoint.

## Consequences

Production and integration test host (`Testing`) do not expose Swagger. Developers use [docs/LOCAL_SETUP.md](../LOCAL_SETUP.md) as the canonical walkthrough.