# ADR-0006: M3 LLM triage pipeline

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

Workers must triage tickets after messaging. Local development and CI need a deterministic path without mandatory cloud LLM credentials.

## Decision

Introduce `ILlmClient` with a pluggable provider. Default **local development provider** (`Llm:Provider=local`) uses an in-process deterministic client—no external API. After `QueuedForTriage`, the worker runs PII redaction, active `PromptVersions` template, JSON-schema-style parsing, and persists `TicketAnalyses` + `LlmCalls`. Route using configurable confidence threshold (default 0.85) to `AutoApproved` or `NeedsReview`. Expose `GET /api/tickets/{id}/analysis`. Production Claude/Anthropic wiring is planned behind the same interface (M4+).

## Consequences

Integration tests use the local provider. Docker runs end-to-end without API keys. Swap providers via configuration when cloud credentials are available.