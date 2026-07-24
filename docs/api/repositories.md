# Repositories API

Base: `/api/v1`

Uses the configured `GitHub:PersonalAccessToken`. No user login.

## `GET /api/v1/repositories`

Lists repositories visible to the token.

**Responses**

| Status | Body |
|--------|------|
| `200` | `RepositorySummaryDto[]` |
| `401` | ProblemDetails (`github_unauthorized`) |
| `429` | ProblemDetails (`github_rate_limited`) |
| `502` | ProblemDetails (`github_error`) |

## `GET /api/v1/repositories/{owner}/{name}`

Returns a single repository.

**Path**

| Param | Rules |
|-------|-------|
| `owner` | required, max 100 |
| `name` | required, max 100 |

**Responses**

| Status | Body |
|--------|------|
| `200` | `RepositoryDetailsDto` |
| `400` | ProblemDetails (`validation_failed`) |
| `404` | ProblemDetails (`github_not_found`) |
