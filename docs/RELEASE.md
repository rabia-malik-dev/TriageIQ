# Release v0.1.0

TriageIQ reaches milestones **M0–M9** with review UI, evaluation gate, observability profile, GHCR release workflow, and private study-guide tooling.

## Quick start (10 minutes)

1. Install Docker Desktop.
2. `git clone https://github.com/rabia-malik-dev/TriageIQ.git`
3. `copy .env.example .env` — set `JWT_SIGNING_KEY` (32+ chars) and SQL password.
4. `docker compose up --build -d`
5. API: http://localhost:8080/swagger · Web: http://localhost:8081 · Health: http://localhost:8080/health/ready
6. Optional observability: `docker compose --profile observability up -d` → Jaeger :16686, Grafana :3000

## Test and evaluate

```bash
dotnet test TriageIQ.slnx -c Release
python tools/eval/generate_triage_v1.py
dotnet run --project src/TriageIQ.Evals -c Release -- replay triage-v1
cd web && npm ci && npm run build
```

## GHCR images (tag `v0.1.0`)

GitHub Actions `release.yml` publishes:

- `ghcr.io/rabia-malik-dev/triageiq-api`
- `ghcr.io/rabia-malik-dev/triageiq-worker`
- `ghcr.io/rabia-malik-dev/triageiq-web`

## M9 study guides (private)

Sources stay in gitignored `study/src/`. Seed from `study-templates/src/`, then:

```bash
pip install reportlab
python tools/study/build_pdfs.py
```

Output: `study-guides/` (gitignored).