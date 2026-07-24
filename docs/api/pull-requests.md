# Pull Requests API

Base: `/api/v1`

Nested under repositories for RESTful hierarchy.

## `GET /api/v1/repositories/{owner}/{name}/pull-requests`

Lists pull requests (all states, recently updated first).

**Responses:** `200` list of `PullRequestSummaryDto`; `400` / `401` / `404` / `429` / `502` ProblemDetails.

## `GET /api/v1/repositories/{owner}/{name}/pull-requests/{number}`

Full PR details including files, commits, and comments.

| Param | Rules |
|-------|-------|
| `number` | integer &gt; 0 |

## `GET /api/v1/repositories/{owner}/{name}/pull-requests/{number}/analysis`

Builds a deterministic `ReviewContext` (no OpenAI call).

Pass the response body to [`POST /api/v1/reviews`](reviews.md).

**Responses:** `200` `ReviewContext`; validation and GitHub ProblemDetails as above.
