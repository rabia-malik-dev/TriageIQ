# ADR-0007: M4 triage guardrails

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

M3 triage calls an LLM with basic confidence routing. Production-style triage needs pre- and post-LLM checks: prompt injection, PII handling, schema validation, malformed JSON repair, and cost/confidence review routing—without changing the core `ILlmClient` contract from M3.

## Decision

Add `TriageIQ.Application.Guardrails`:

- **Pre-LLM:** `PromptInjectionScanner` blocks suspicious phrases; no LLM call. Persist `TicketAnalysis` with `ReviewReason` and a synthetic `LlmCall` (`provider: guardrail`).
- **PII:** `PiiRedactor` (email, phone, SSN, payment card) runs before prompt build; stored on `Ticket.BodyRedacted`.
- **Post-LLM:** Parse JSON; `TriageJsonRepair` for fenced/embedded JSON; one repair completion using `TriagePromptBuilder.RepairSystemPrompt` on parse failure.
- **Validation:** `TriageOutputValidator` enforces category/summary bounds and allowed priorities.
- **Routing:** `PostTriageGuardrails` maps violations (`prompt_injection`, `invalid_schema`, `low_confidence`, `cost_budget_exceeded`, `llm_failed`) to `NeedsReview` or `Failed`; store joined codes on `TicketAnalyses.ReviewReason`.
- **Config:** `Triage:ConfidenceThreshold` (default 0.85), `Triage:MaxEstimatedCostUsd` (default 0.25).

Unit tests cover each guardrail component; integration test covers injection → `NeedsReview` without a real LLM call.

## Consequences

Tickets with injection never hit the LLM locally—saves cost and reduces risk. Uncertain or expensive triage surfaces `reviewReason` on `GET /api/tickets/{id}/analysis`. Cloud LLM providers (post-M4) plug into the same pipeline unchanged.