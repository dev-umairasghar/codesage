import { describe, expect, it } from 'vitest'
import { normalizeRisk } from '@/components/review/RiskBadge'
import { riskToScore } from '@/lib/utils'
import { ApiError, toApiError } from '@/api/http'

describe('risk helpers', () => {
  it('normalizes numeric and string risk', () => {
    expect(normalizeRisk('High')).toBe('High')
    expect(normalizeRisk(3)).toBe('High')
  })

  it('maps risk to score', () => {
    expect(riskToScore('Low')).toBe(90)
    expect(riskToScore('Critical')).toBe(15)
  })
})

describe('toApiError', () => {
  it('wraps generic errors', () => {
    const error = toApiError(new Error('network down'))
    expect(error).toBeInstanceOf(ApiError)
    expect(error.message).toBe('network down')
  })

  it('passes through ApiError', () => {
    const original = new ApiError('x', { errorCode: 'validation_failed' }, 400)
    expect(toApiError(original)).toBe(original)
  })
})
