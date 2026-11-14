# TriageIQ Web (M5)

Angular 19 standalone review UI: login, human review queue, and per-ticket approve.

## Prerequisites

- Node.js 20+ (22 recommended)
- TriageIQ API on port **8080** for local dev (`docker compose up` from repo root)

## Install and build

```bash
cd web
npm ci
npm run build
```

Production output: `dist/triageiq-web/`.

## Development server

```bash
npm ci
npm start
```

Open http://localhost:4200. The dev build calls the API at `http://localhost:8080` (`src/environments/environment.development.ts`).

## Docker image

Build from this directory (nginx proxies `/api` to `triageiq-api:8080`):

```bash
docker build -t triageiq-web .
```

## API routes used

| Method | Path |
|--------|------|
| POST | `/api/auth/login` |
| GET | `/api/reviews/queue` |
| POST | `/api/reviews/{ticketId}/approve` |

JWT bearer token is attached via `HttpClient` interceptor after login.