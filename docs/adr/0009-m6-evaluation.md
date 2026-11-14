# ADR-0009: M6 evaluation harness

- **Status:** Accepted
- **Date:** 2026-11-14

## Decision

Ship `eval/datasets/triage-v1.jsonl` (100 synthetic tickets), `TriageIQ.Evals` replay CLI, quality gate thresholds in `eval/baselines/quality-gate.json`, and CI step `dotnet run ... replay triage-v1`. `EvalRuns` persist API-triggered runs.

## Consequences

CI fails on quality regression; live runs use `eval-live.yml` workflow_dispatch.