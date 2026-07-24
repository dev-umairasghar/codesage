import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQueryClient } from '@tanstack/react-query'
import { RiskBadge, normalizeRisk } from '@/components/review/RiskBadge'
import { EmptyState } from '@/components/shared/states'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import { queryKeys, useSessionReviews } from '@/hooks/useApi'
import { formatDate, riskToScore } from '@/lib/utils'
import { clearSessionReviews } from '@/utils/sessionReviews'

const filtersSchema = z.object({
  query: z.string(),
  risk: z.enum(['all', 'Low', 'Medium', 'High', 'Critical', 'Unknown']),
  sort: z.enum(['newest', 'oldest', 'risk']),
})

type Filters = z.infer<typeof filtersSchema>

export function ReviewsPage() {
  const { data = [] } = useSessionReviews()
  const queryClient = useQueryClient()
  const form = useForm<Filters>({
    resolver: zodResolver(filtersSchema),
    defaultValues: { query: '', risk: 'all', sort: 'newest' },
  })
  const [applied, setApplied] = useState<Filters>(form.getValues())
  const [page, setPage] = useState(1)
  const pageSize = 5

  const filtered = useMemo(() => {
    let list = [...data]
    const needle = applied.query.trim().toLowerCase()
    if (needle) {
      list = list.filter(
        (item) =>
          item.pullRequestTitle.toLowerCase().includes(needle) ||
          `${item.owner}/${item.repo}`.toLowerCase().includes(needle) ||
          item.authorLogin.toLowerCase().includes(needle),
      )
    }
    if (applied.risk !== 'all') {
      list = list.filter((item) => normalizeRisk(item.report.overallRisk) === applied.risk)
    }
    list.sort((a, b) => {
      if (applied.sort === 'oldest') {
        return a.createdAt.localeCompare(b.createdAt)
      }
      if (applied.sort === 'risk') {
        return riskToScore(normalizeRisk(a.report.overallRisk)) - riskToScore(normalizeRisk(b.report.overallRisk))
      }
      return b.createdAt.localeCompare(a.createdAt)
    })
    return list
  }, [data, applied])

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize))
  const pageItems = filtered.slice((page - 1) * pageSize, page * pageSize)

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold text-ink">Reviews</h1>
        <p className="max-w-2xl text-sm text-ink-muted">
          Session history only — the API is stateless. Reviews generated in this browser tab are kept
          in session storage until you clear them or close the tab.
        </p>
      </header>

      <form
        className="grid gap-2 sm:grid-cols-4"
        onSubmit={form.handleSubmit((values) => {
          setApplied(values)
          setPage(1)
        })}
      >
        <input
          {...form.register('query')}
          placeholder="Search title, repo, author"
          className="h-10 rounded-md border border-border bg-panel px-3 text-sm sm:col-span-2"
        />
        <select {...form.register('risk')} className="h-10 rounded-md border border-border bg-panel px-3 text-sm">
          <option value="all">All risks</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Critical">Critical</option>
          <option value="Unknown">Unknown</option>
        </select>
        <select {...form.register('sort')} className="h-10 rounded-md border border-border bg-panel px-3 text-sm">
          <option value="newest">Newest</option>
          <option value="oldest">Oldest</option>
          <option value="risk">Risk (score asc)</option>
        </select>
        <div className="flex gap-2 sm:col-span-4">
          <Button type="submit" variant="secondary">
            Apply
          </Button>
          <Button
            type="button"
            variant="ghost"
            onClick={() => {
              clearSessionReviews()
              void queryClient.invalidateQueries({ queryKey: queryKeys.sessionReviews })
            }}
          >
            Clear session
          </Button>
        </div>
      </form>

      {pageItems.length === 0 ? (
        <EmptyState
          title="No reviews in this session"
          description="Run an AI review from a pull request page to populate this list."
          action={
            <Link to="/repositories" className="text-sm text-accent">
              Browse repositories
            </Link>
          }
        />
      ) : (
        <div className="space-y-3">
          {pageItems.map((item) => (
            <Card key={item.id}>
              <div className="flex flex-wrap items-center gap-2">
                <RiskBadge risk={item.report.overallRisk} />
                <span className="font-mono text-xs text-ink-subtle">
                  {item.owner}/{item.repo}#{item.pullRequestNumber}
                </span>
              </div>
              <CardTitle className="mt-2 text-base">{item.pullRequestTitle}</CardTitle>
              <CardDescription>
                {item.authorLogin} · {formatDate(item.createdAt)} · score{' '}
                {riskToScore(normalizeRisk(item.report.overallRisk))}
              </CardDescription>
              <Link
                to={`/repositories/${encodeURIComponent(item.owner)}/${encodeURIComponent(item.repo)}/pull-requests/${item.pullRequestNumber}`}
                className="mt-3 inline-block text-sm"
              >
                Open PR review page
              </Link>
            </Card>
          ))}
        </div>
      )}

      {filtered.length > pageSize ? (
        <div className="flex items-center gap-3 text-sm text-ink-muted">
          <Button
            type="button"
            variant="secondary"
            size="sm"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
          >
            Previous
          </Button>
          <span>
            Page {page} / {totalPages}
          </span>
          <Button
            type="button"
            variant="secondary"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </Button>
        </div>
      ) : null}
    </div>
  )
}
