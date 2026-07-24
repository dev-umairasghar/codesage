import { Badge } from '@/components/ui/badge'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import { enumLabel } from '@/lib/utils'
import type { ReviewFinding } from '@/types/api'

export function FindingCard({ finding }: { finding: ReviewFinding }) {
  return (
    <Card className="bg-panel-elevated/70">
      <div className="flex flex-wrap items-center gap-2">
        <Badge>{enumLabel(String(finding.severity))}</Badge>
        <Badge>{enumLabel(String(finding.category))}</Badge>
      </div>
      <CardTitle className="mt-2 text-sm">{finding.title}</CardTitle>
      <CardDescription className="whitespace-pre-wrap">{finding.description}</CardDescription>
      {finding.whyItMatters ? (
        <p className="mt-2 text-xs text-ink-muted">
          <span className="text-ink-subtle">Why it matters:</span> {finding.whyItMatters}
        </p>
      ) : null}
      {finding.filePath ? (
        <p className="mt-2 font-mono text-[11px] text-accent">
          {finding.filePath}
          {finding.startLine != null ? `:${finding.startLine}` : ''}
          {finding.endLine != null && finding.endLine !== finding.startLine
            ? `-${finding.endLine}`
            : ''}
        </p>
      ) : null}
      {finding.suggestion ? (
        <p className="mt-2 rounded-md bg-accent-soft px-2 py-1.5 text-xs text-ink">
          {finding.suggestion}
        </p>
      ) : null}
    </Card>
  )
}

export function IssueCard({ finding }: { finding: ReviewFinding }) {
  return <FindingCard finding={finding} />
}

export function RecommendationCard({ text }: { text: string }) {
  return (
    <li className="rounded-lg border border-border bg-panel-elevated/60 px-3 py-2 text-sm text-ink">
      {text}
    </li>
  )
}
