# Reviews API

Base: `/api/v1`

## `POST /api/v1/reviews`

Runs the configured OpenAI model against a `ReviewContext` and returns a `ReviewReport`.

Stateless — nothing is stored.

### Request

`Content-Type: application/json` — body is a `ReviewContext` (typically from PR analysis).

Required highlights (FluentValidation):

- `repository.fullName`, `repository.name`
- `pullRequest.number` &gt; 0, title, base/head refs
- `author.login`
- `commits`, `changedFiles`, `statistics`, `languageBreakdown`
- `summary`

### Responses

| Status | Meaning |
|--------|---------|
| `200` | `ReviewReport` |
| `400` | Validation failed |
| `429` | AI rate limited |
| `502` | Invalid/upstream AI response |
| `503` | AI not configured |
| `504` | AI timeout |

### Example flow

1. `GET .../pull-requests/{number}/analysis` → `ReviewContext`
2. `POST /api/v1/reviews` with that body → `ReviewReport`
