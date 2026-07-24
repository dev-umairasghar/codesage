import { http, toApiError } from '@/api/http'
import type { ReviewContext, ReviewReport } from '@/types/api'

export const reviewsApi = {
  async analyze(owner: string, name: string, number: number): Promise<ReviewContext> {
    try {
      const { data } = await http.get<ReviewContext>(
        `/api/v1/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(name)}/pull-requests/${number}/analysis`,
      )
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },

  async create(context: ReviewContext): Promise<ReviewReport> {
    try {
      const { data } = await http.post<ReviewReport>('/api/v1/reviews', context)
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },
}
