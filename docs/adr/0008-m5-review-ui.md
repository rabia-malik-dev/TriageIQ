# ADR-0008: M5 review workflow and Angular UI

- **Status:** Accepted
- **Date:** 2026-11-14

## Decision

Add `ReviewActions` + audit logging, REST under `/api/reviews`, and an Angular 19 standalone app (`web/`) served by Nginx with JWT login and review queue approve flow. Playwright smoke spec lives in `tests/e2e/` (opt-in via `TRIAGEIQ_E2E=1`).

## Consequences

Reviewers complete approve/edit/reject in the browser; edits append to `eval/datasets/feedback-export.jsonl` for future eval augmentation.