import { RiskBadge, ScoreDisplay, normalizeRisk } from '@/components/review/RiskBadge'
import { FindingCard, RecommendationCard } from '@/components/review/FindingCard'
import { ExpandableSection } from '@/components/ui/expandable-section'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import type { ReviewReport } from '@/types/api'

export function ReviewSummaryCard({ report }: { report: ReviewReport }) {
  return (
    <Card className="grid gap-4 md:grid-cols-[1fr_auto]">
      <div>
        <div className="flex flex-wrap items-center gap-2">
          <RiskBadge risk={report.overallRisk} />
          <span className="font-mono text-xs text-ink-subtle">{report.model}</span>
        </div>
        <CardTitle className="mt-3 text-lg">Review summary</CardTitle>
        <CardDescription className="mt-2 whitespace-pre-wrap text-base leading-relaxed text-ink-muted">
          {report.summary}
        </CardDescription>
      </div>
      <ScoreDisplay risk={report.overallRisk} />
    </Card>
  )
}

export function ReviewFindingsPanels({ report }: { report: ReviewReport }) {
  return (
    <div className="space-y-3">
      <ExpandableSection title="Architecture" count={report.architectureConcerns.length} defaultOpen>
        <FindingList findings={report.architectureConcerns} empty="No architecture findings." />
      </ExpandableSection>
      <ExpandableSection title="Security" count={report.securityConcerns.length} defaultOpen>
        <FindingList findings={report.securityConcerns} empty="No security findings." />
      </ExpandableSection>
      <ExpandableSection title="Performance" count={report.performanceConcerns.length}>
        <FindingList findings={report.performanceConcerns} empty="No performance findings." />
      </ExpandableSection>
      <ExpandableSection title="Maintainability" count={report.maintainability.length}>
        <FindingList findings={report.maintainability} empty="No maintainability findings." />
      </ExpandableSection>
      <ExpandableSection title="Issues" count={report.issues.length} defaultOpen>
        <FindingList findings={report.issues} empty="No issues reported." />
      </ExpandableSection>
      <ExpandableSection title="Testing" count={report.missingTests.length}>
        {report.missingTests.length === 0 ? (
          <p className="text-sm text-ink-muted">No missing tests called out.</p>
        ) : (
          <ul className="space-y-2">
            {report.missingTests.map((item) => (
              <RecommendationCard key={item} text={item} />
            ))}
          </ul>
        )}
      </ExpandableSection>
      <ExpandableSection title="Recommendations" count={report.recommendations.length} defaultOpen>
        {report.recommendations.length === 0 ? (
          <p className="text-sm text-ink-muted">No recommendations.</p>
        ) : (
          <ul className="space-y-2">
            {report.recommendations.map((item) => (
              <RecommendationCard key={item} text={item} />
            ))}
          </ul>
        )}
      </ExpandableSection>
      <ExpandableSection title="Positive findings" count={report.positiveFindings.length}>
        {report.positiveFindings.length === 0 ? (
          <p className="text-sm text-ink-muted">None listed.</p>
        ) : (
          <ul className="space-y-2">
            {report.positiveFindings.map((item) => (
              <RecommendationCard key={item} text={item} />
            ))}
          </ul>
        )}
      </ExpandableSection>
      <p className="text-xs text-ink-subtle">
        Risk {normalizeRisk(report.overallRisk)} · duration {report.duration}
        {report.totalTokens != null ? ` · ${report.totalTokens} tokens` : ''}
      </p>
    </div>
  )
}

function FindingList({
  findings,
  empty,
}: {
  findings: ReviewReport['issues']
  empty: string
}) {
  if (findings.length === 0) {
    return <p className="text-sm text-ink-muted">{empty}</p>
  }
  return (
    <div className="space-y-3">
      {findings.map((finding) => (
        <FindingCard key={`${finding.title}-${finding.filePath}`} finding={finding} />
      ))}
    </div>
  )
}
