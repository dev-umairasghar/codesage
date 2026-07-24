import { http, toApiError } from '@/api/http'
import type { ConfigurationSummary, HealthResponse, SystemStatus } from '@/types/api'

export const systemApi = {
  async health(): Promise<HealthResponse> {
    try {
      const { data } = await http.get<HealthResponse>('/api/v1/health')
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },

  async status(): Promise<SystemStatus> {
    try {
      const { data } = await http.get<SystemStatus>('/api/v1/system/status')
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },

  async configuration(): Promise<ConfigurationSummary> {
    try {
      const { data } = await http.get<ConfigurationSummary>('/api/v1/configuration')
      return data
    } catch (error) {
      throw toApiError(error)
    }
  },
}
