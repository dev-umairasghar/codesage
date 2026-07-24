import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RiskBadge, ScoreDisplay } from '@/components/review/RiskBadge'
import { ReviewSummaryCard } from '@/components/review/ReviewSummaryCard'
import { ErrorMessage, EmptyState, LoadingSkeleton } from '@/components/shared/states'
import { ApiError } from '@/api/http'
import type { ReviewReport } from '@/types/api'

const sampleReport: ReviewReport = {
  summary: 'Looks solid with one auth edge case.',
  overallRisk: 'Medium',
  positiveFindings: ['Clear diff'],
  issues: [],
  recommendations: ['Add a test'],
  missingTests: [],
  securityConcerns: [],
  performanceConcerns: [],
  maintainability: [],
  architectureConcerns: [],
  model: 'gpt-4o-mini',
  promptTokens: 10,
  completionTokens: 20,
  totalTokens: 30,
  duration: '00:00:01',
}

describe('review UI', () => {
  it('renders risk badge and score', () => {
    render(
      <>
        <RiskBadge risk="High" />
        <ScoreDisplay risk="High" />
      </>,
    )
    expect(screen.getAllByText(/high risk/i).length).toBeGreaterThan(0)
    expect(screen.getByText('40')).toBeInTheDocument()
  })

  it('renders review summary', () => {
    render(<ReviewSummaryCard report={sampleReport} />)
    expect(screen.getByText(/looks solid/i)).toBeInTheDocument()
    expect(screen.getByText(/gpt-4o-mini/i)).toBeInTheDocument()
  })
})

describe('shared states', () => {
  it('renders loading skeleton', () => {
    render(<LoadingSkeleton rows={2} />)
    expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument()
  })

  it('renders empty state', () => {
    render(<EmptyState title="Nothing here" description="Try again" />)
    expect(screen.getByText('Nothing here')).toBeInTheDocument()
  })

  it('renders error message from ApiError', () => {
    render(
      <ErrorMessage
        error={new ApiError('Missing token', { title: 'GitHub authorization failed', errorCode: 'github_unauthorized' }, 401)}
      />,
    )
    expect(screen.getByRole('alert')).toBeInTheDocument()
    expect(screen.getByText(/missing token/i)).toBeInTheDocument()
    expect(screen.getByText(/github_unauthorized/i)).toBeInTheDocument()
  })
})
