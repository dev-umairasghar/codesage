import { Card, CardDescription, CardTitle } from '@/components/ui/card'

export function AboutPage() {
  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold text-ink">About</h1>
        <p className="max-w-2xl text-sm text-ink-muted">
          CodeSage is a local-first, open-source AI pull request reviewer. The UI talks only to your
          local CodeSage API — never directly to GitHub or OpenAI.
        </p>
      </header>

      <div className="grid gap-3 md:grid-cols-2">
        <Card>
          <CardTitle>Workflow</CardTitle>
          <CardDescription className="mt-2 space-y-1">
            <p>1. Browse repositories</p>
            <p>2. Pick a pull request</p>
            <p>3. Run AI review</p>
            <p>4. Read findings by category</p>
          </CardDescription>
        </Card>
        <Card>
          <CardTitle>Stack</CardTitle>
          <CardDescription className="mt-2 space-y-1">
            <p>React + TypeScript + Vite</p>
            <p>TanStack Query + Axios</p>
            <p>Tailwind CSS + React Hook Form + Zod</p>
            <p>ASP.NET Core API on /api/v1</p>
          </CardDescription>
        </Card>
      </div>
    </div>
  )
}
