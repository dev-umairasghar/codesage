# CodeSage Web

React frontend for the local-first CodeSage API.

## Stack

- React 19 + TypeScript + Vite
- React Router
- TanStack Query + Axios
- Tailwind CSS v4
- React Hook Form + Zod

## Prerequisites

1. CodeSage API running on `http://localhost:5080` (see repo root README)
2. Node.js 20+

## Develop

```bash
cd web
npm install
npm run dev
```

Open `http://localhost:5173`. Vite proxies `/api` → `http://localhost:5080`.

Optional: set `VITE_API_BASE_URL` to point at another API host (requires CORS on the API).

## Scripts

| Command | Purpose |
|---------|---------|
| `npm run dev` | Dev server |
| `npm run build` | Production build |
| `npm run preview` | Preview build |
| `npm test` | Vitest |

## Workflow

Repositories → Pull requests → Run AI review → View findings → Session Reviews list

Reviews are **not** persisted by the API. The Reviews page shows sessionStorage history for the current browser tab.
