import { useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Loader2 } from 'lucide-react'
import { ReviewFindingsPanels, ReviewSummaryCard } from '@/components/review/ReviewSummaryCard'
import { EmptyState, ErrorMessage, LoadingSkeleton } from '@/components/shared/states'
import { Button } from '@/components/ui/button'
import { ExpandableSection } from '@/components/ui/expandable-section'
import { Badge } from '@/components/ui/badge'
import { usePullRequest, useRunReview } from '@/hooks/useApi'
import { formatDate } from '@/lib/utils'
import type { ReviewReport } from '@/types/api'

export function ReviewPage() {
  const { owner = '', repo = '', number: numberParam = '0' } = useParams()
  const number = Number(numberParam)
  const pullRequest = usePullRequest(owner, repo, number)
  const runReview = useRunReview()
  const [report, setReport] = useState<ReviewReport | null>(null)

  const statusLabel = useMemo(() => {
    if (runReview.isPending) {
      return 'Running analysis + AI review…'
    }
    if (report) {
      return 'Review complete'
    }
    return 'Ready to review'
  }, [runReview.isPending, report])

  async function handleRun() {
    if (!pullRequest.data) {
      return
    }
    const result = await runReview.mutateAsync({
      owner,
      repo,
      number,
      pullRequestTitle: pullRequest.data.title,
      authorLogin: pullRequest.data.authorLogin,
    })
    setReport(result.report)
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Link
          to={`/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(repo)}`}
          className="text-xs text-ink-subtle no-underline hover:text-accent"
        >
          ← Pull requests
        </Link>
        {pullRequest.isLoading ? <LoadingSkeleton rows={2} /> : null}
        {pullRequest.isError ? (
          <ErrorMessage error={pullRequest.error} onRetry={() => void pullRequest.refetch()} />
        ) : null}
        {pullRequest.data ? (
          <>
            <div className="flex flex-wrap items-center gap-2">
              <Badge className="font-mono">#{pullRequest.data.number}</Badge>
              <Badge>{pullRequest.data.state}</Badge>
              <span className="text-xs text-ink-subtle">{statusLabel}</span>
            </div>
            <h1 className="text-2xl font-semibold text-ink">{pullRequest.data.title}</h1>
            <p className="text-sm text-ink-muted">
              {pullRequest.data.authorLogin} · {pullRequest.data.baseRef} ← {pullRequest.data.headRef}{' '}
              · updated {formatDate(pullRequest.data.updatedAt)} ·{' '}
              {pullRequest.data.changedFiles.length} files
            </p>
          </>
        ) : null}
      </div>

      <div className="flex flex-wrap gap-2">
        <Button
          type="button"
          disabled={!pullRequest.data || runReview.isPending}
          onClick={() => void handleRun()}
        >
          {runReview.isPending ? (
            <>
              <Loader2 className="size-4 animate-spin" aria-hidden />
              Generating review
            </>
          ) : (
            'Run AI review'
          )}
        </Button>
        {report ? (
          <Button type="button" variant="secondary" onClick={() => setReport(null)}>
            Clear result
          </Button>
        ) : null}
      </div>

      {runReview.isError ? (
        <ErrorMessage error={runReview.error} onRetry={() => void handleRun()} />
      ) : null}

      {!report && !runReview.isPending ? (
        <EmptyState
          title="No review yet"
          description="CodeSage will analyze the PR via the API, then call OpenAI through the backend. Nothing is stored server-side."
        />
      ) : null}

      {report ? (
        <div className="space-y-4">
          <ReviewSummaryCard report={report} />
          <ReviewFindingsPanels report={report} />
        </div>
      ) : null}

      {pullRequest.data ? (
        <ExpandableSection title="Changed files" count={pullRequest.data.changedFiles.length}>
          <ul className="space-y-2">
            {pullRequest.data.changedFiles.map((file) => (
              <li
                key={file.filename}
                className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-panel-elevated/50 px-3 py-2 font-mono text-xs"
              >
                <span className="text-ink">{file.filename}</span>
                <span className="text-ink-muted">
                  {file.status} · +{file.additions} −{file.deletions}
                </span>
              </li>
            ))}
          </ul>
        </ExpandableSection>
      ) : null}
    </div>
  )
}
