# CodeSage

Local-first, **stateless** AI pull request review assistant for developers.

Configure secrets once, run against GitHub + OpenAI, get structured reviews — no database, no accounts, no OAuth.

## Stages

| Stage | Status |
|-------|--------|
| 1. Foundation | Done |
| 2. GitHub REST (PAT) | Done |
| 3. Deterministic PR analysis | Done |
| 4. OpenAI AI review engine | Done |
| 5. Options pattern, validation, health/diagnostics | Done |
| 6. Public API polish (v1, Swagger/OpenAPI, ProblemDetails) | Done |
| 7. React frontend | Done |

**Not in scope yet:** auth, database, persistence, RAG, MCP, agents, Semantic Kernel, background jobs.

## Quick start

### API

```bash
dotnet restore
dotnet build

cd src/CodeSage.Api
dotnet user-secrets set "GitHub:PersonalAccessToken" "ghp_your_token"
dotnet user-secrets set "OpenAI:ApiKey" "sk-your_key"

dotnet run --project src/CodeSage.Api
```

API listens on `http://localhost:5080` (see `launchSettings.json`).

### Web UI

```bash
cd web
npm install
npm run dev
```

Open `http://localhost:5173` — Vite proxies `/api` to the API.

- Swagger UI (Development): `/swagger`
- OpenAPI document: `/swagger/v1/swagger.json` (also [`docs/openapi-v1.json`](docs/openapi-v1.json))
- Health: `GET /api/v1/health` (alias: `/api/health`)
- Config summary: `GET /api/v1/configuration`

**Recommended for local secrets:** .NET User Secrets — see [docs/Configuration.md](docs/Configuration.md).

## API (v1)

| Method | Path | Purpose |
|--------|------|---------|
| `GET` | `/api/v1/health` | Liveness |
| `GET` | `/api/v1/system/status` | Diagnostics |
| `GET` | `/api/v1/configuration` | Public config summary |
| `GET` | `/api/v1/repositories` | List repos |
| `GET` | `/api/v1/repositories/{owner}/{name}` | Repo details |
| `GET` | `/api/v1/repositories/{owner}/{name}/pull-requests` | List PRs |
| `GET` | `/api/v1/repositories/{owner}/{name}/pull-requests/{number}` | PR details |
| `GET` | `/api/v1/repositories/{owner}/{name}/pull-requests/{number}/analysis` | `ReviewContext` |
| `POST` | `/api/v1/reviews` | Stateless AI review |

Full docs: [docs/api.md](docs/api.md) · Web app: [web/README.md](web/README.md)

## Solution layout

```
src/          # .NET API (Clean Architecture)
web/          # React + Vite frontend
tests/        # .NET unit + integration tests
docs/         # API + configuration docs
```

## Tests

```bash
dotnet test
cd web && npm test
```

## Design notes

- Public API under `/api/v1` — ready for typed client generation from OpenAPI
- FluentValidation + MediatR pipeline — invalid requests never reach handlers
- Centralized ProblemDetails (`errorCode`, `traceId`)
- Frontend talks only to CodeSage REST — never GitHub/OpenAI directly
- Response compression + structured request logging (never logs secrets)
