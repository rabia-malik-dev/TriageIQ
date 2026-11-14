# ADR-0006: M3 fake LLM triage pipeline

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

Workers must triage tickets after messaging, but local dev and CI cannot depend on paid LLM APIs.

## Decision

Introduce `ILlmClient` with default `FakeLlmClient` when `Llm:Provider=fake`. After `QueuedForTriage`, worker runs PII redaction, active `PromptVersions` template, JSON-schema-style parsing, and persists `TicketAnalyses` + `LlmCalls`. Route using configurable confidence threshold (default 0.85) to `AutoApproved` or `NeedsReview`. Expose `GET /api/tickets/{id}/analysis`.

## Consequences

Anthropic/real provider wiring is deferred until keys and guardrails (M4) are in place. Fake client uses deterministic heuristics (e.g. “payment” → billing).