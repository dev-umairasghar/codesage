import { Link, useParams } from 'react-router-dom'
import { PullRequestCard } from '@/components/review/PullRequestCard'
import { EmptyState, ErrorMessage, LoadingSkeleton } from '@/components/shared/states'
import { Badge } from '@/components/ui/badge'
import { usePullRequests, useRepository } from '@/hooks/useApi'

export function PullRequestsPage() {
  const { owner = '', repo = '' } = useParams()
  const repository = useRepository(owner, repo)
  const pullRequests = usePullRequests(owner, repo)

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Link to="/repositories" className="text-xs text-ink-subtle no-underline hover:text-accent">
          ← Repositories
        </Link>
        <h1 className="font-mono text-2xl font-semibold text-ink">
          {owner}/{repo}
        </h1>
        {repository.data ? (
          <div className="flex flex-wrap gap-2 text-sm text-ink-muted">
            <Badge>{repository.data.defaultBranch}</Badge>
            {repository.data.language ? <Badge>{repository.data.language}</Badge> : null}
            <span>{repository.data.openIssuesCount} open issues</span>
            <span>{pullRequests.data?.length ?? '—'} pull requests listed</span>
          </div>
        ) : null}
      </div>

      {pullRequests.isLoading ? <LoadingSkeleton rows={4} /> : null}
      {pullRequests.isError ? (
        <ErrorMessage error={pullRequests.error} onRetry={() => void pullRequests.refetch()} />
      ) : null}

      {!pullRequests.isLoading && !pullRequests.isError && (pullRequests.data?.length ?? 0) === 0 ? (
        <EmptyState
          title="No pull requests"
          description="This repository has no pull requests visible to the configured token."
        />
      ) : null}

      <div className="space-y-3">
        {pullRequests.data?.map((pr) => (
          <PullRequestCard key={pr.number} owner={owner} repo={repo} pullRequest={pr} />
        ))}
      </div>
    </div>
  )
}
