import { Link } from 'react-router-dom'
import { Badge } from '@/components/ui/badge'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import { cn } from '@/lib/utils'
import { formatDate } from '@/lib/utils'
import type { PullRequestSummary } from '@/types/api'

export function PullRequestCard({
  owner,
  repo,
  pullRequest,
}: {
  owner: string
  repo: string
  pullRequest: PullRequestSummary
}) {
  const to = `/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(repo)}/pull-requests/${pullRequest.number}`

  return (
    <Card className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-2">
          <Badge className="font-mono">#{pullRequest.number}</Badge>
          <Badge
            className={
              pullRequest.state === 'open'
                ? 'border-success/30 text-success'
                : 'border-ink-subtle/40 text-ink-muted'
            }
          >
            {pullRequest.draft ? 'Draft' : pullRequest.state}
          </Badge>
        </div>
        <CardTitle className="mt-2 truncate">{pullRequest.title}</CardTitle>
        <CardDescription>
          {pullRequest.authorLogin} · created {formatDate(pullRequest.createdAt)} · updated{' '}
          {formatDate(pullRequest.updatedAt)}
        </CardDescription>
      </div>
      <Link
        to={to}
        className={cn(
          'inline-flex h-10 shrink-0 items-center justify-center rounded-md bg-accent px-4 text-sm font-medium text-canvas no-underline hover:bg-accent/90 hover:no-underline',
        )}
      >
        Review PR
      </Link>
    </Card>
  )
}
