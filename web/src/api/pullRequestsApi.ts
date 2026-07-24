import { http, toApiError } from '@/api/http'
import type { PullRequestDetails, PullRequestSummary } from '@/types/api'

export const pullRequestsApi = {
  async list(owner: string, name: string): Promise<PullRequestSummary[]> {
    try {
      const { data } = await http.get<PullRequestSummary[]>(
        `/api/v1/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(name)}/pull-requests`,
      )
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },

  async get(owner: string, name: string, number: number): Promise<PullRequestDetails> {
    try {
      const { data } = await http.get<PullRequestDetails>(
        `/api/v1/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(name)}/pull-requests/${number}`,
      )
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },
}
