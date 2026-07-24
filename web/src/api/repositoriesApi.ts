import { http, toApiError } from '@/api/http'
import type { RepositoryDetails, RepositorySummary } from '@/types/api'

export const repositoriesApi = {
  async list(): Promise<RepositorySummary[]> {
    try {
      const { data } = await http.get<RepositorySummary[]>('/api/v1/repositories')
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },

  async get(owner: string, name: string): Promise<RepositoryDetails> {
    try {
      const { data } = await http.get<RepositoryDetails>(
        `/api/v1/repositories/${encodeURIComponent(owner)}/${encodeURIComponent(name)}`,
      )
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },
}
