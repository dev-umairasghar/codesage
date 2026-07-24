import { cn, enumLabel, riskToScore } from '@/lib/utils'
import type { ReviewRiskLevel } from '@/types/api'

const riskStyles: Record<string, string> = {
  Low: 'border-success/40 bg-success/10 text-success',
  Medium: 'border-warning/40 bg-warning/10 text-warning',
  High: 'border-orange-400/40 bg-orange-400/10 text-orange-300',
  Critical: 'border-danger/40 bg-danger/10 text-danger',
  Unknown: 'border-border text-ink-muted',
}

export function normalizeRisk(risk: ReviewRiskLevel | number | string): string {
  if (typeof risk === 'number') {
    const map = ['Unknown', 'Low', 'Medium', 'High', 'Critical'] as const
    return map[risk] ?? 'Unknown'
  }
  return enumLabel(risk)
}

export function RiskBadge({ risk }: { risk: ReviewRiskLevel | number | string }) {
  const label = normalizeRisk(risk)
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-md border px-2 py-0.5 font-mono text-[11px] uppercase tracking-wide',
        riskStyles[label] ?? riskStyles.Unknown,
      )}
    >
      {label} risk
    </span>
  )
}

export function ScoreDisplay({ risk }: { risk: ReviewRiskLevel | number | string }) {
  const label = normalizeRisk(risk)
  const score = riskToScore(label)
  return (
    <div className="rounded-xl border border-border bg-panel-elevated px-4 py-3">
      <p className="text-xs uppercase tracking-wide text-ink-subtle">Overall score</p>
      <p className="mt-1 font-mono text-3xl font-semibold text-ink">{score}</p>
      <p className="mt-1 text-xs text-ink-muted">Derived from {label} risk</p>
    </div>
  )
}
