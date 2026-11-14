# ADR-0012: M9 private study guides

- **Status:** Accepted
- **Date:** 2026-11-14

## Decision

Keep `study/` and `study-guides/` gitignored. Provide `tools/study/build_pdfs.py` (ReportLab) and committed `study-templates/src/` samples; developers copy into private `study/src/` per SPEC section 12.

## Consequences

No private PDFs in git history; M9 tooling is reproducible from the public repo.