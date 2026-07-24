# CodeSage API

Public, versioned HTTP API for the local-first CodeSage review assistant.

- **Base path:** `/api/v1`
- **OpenAPI:** [`openapi-v1.json`](openapi-v1.json) (generated from Swagger; regenerate via integration tests)
- **Swagger UI:** `/swagger` (Development)
- **Errors:** RFC 7807 `application/problem+json` with `errorCode` + `traceId`

## Guides

| Area | Doc |
|------|-----|
| Repositories | [api/repositories.md](api/repositories.md) |
| Pull Requests | [api/pull-requests.md](api/pull-requests.md) |
| Reviews | [api/reviews.md](api/reviews.md) |
| Health | [api/health.md](api/health.md) |
| Configuration | [api/configuration.md](api/configuration.md) |
| Local secrets / options | [Configuration.md](Configuration.md) |

## Versioning

All product endpoints live under `/api/v1`. Breaking changes will introduce `/api/v2` without removing v1 until a documented deprecation window.

Unversioned alias: `GET /api/health` (same payload as `GET /api/v1/health`) for probes.

## ProblemDetails shape

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/reviews",
  "traceId": "...",
  "errorCode": "validation_failed",
  "errors": {
    "Context.Repository.FullName": ["Repository.FullName is required (owner/name)."]
  }
}
```

Common `errorCode` values: `validation_failed`, `github_not_found`, `github_unauthorized`, `github_rate_limited`, `ai_timeout`, `ai_configuration`, `internal_error`.

## Client generation

```bash
# After running the API (or integration tests that refresh docs/openapi-v1.json):
npx openapi-typescript docs/openapi-v1.json -o src/api/schema.d.ts
```
