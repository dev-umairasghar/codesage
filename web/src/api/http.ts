import axios, { AxiosError, type AxiosInstance } from 'axios'
import type { ProblemDetails } from '@/types/api'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? ''

export const http: AxiosInstance = axios.create({
  baseURL,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
  timeout: 180_000,
})

export class ApiError extends Error {
  readonly status?: number
  readonly errorCode?: string
  readonly problem?: ProblemDetails

  constructor(message: string, problem?: ProblemDetails, status?: number) {
    super(message)
    this.name = 'ApiError'
    this.problem = problem
    this.status = status ?? problem?.status
    this.errorCode = problem?.errorCode
  }
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error
  }

  if (error instanceof AxiosError) {
    const data = error.response?.data as ProblemDetails | undefined
    const message =
      data?.detail ??
      data?.title ??
      error.message ??
      'Request failed'
    return new ApiError(message, data, error.response?.status)
  }

  if (error instanceof Error) {
    return new ApiError(error.message)
  }

  return new ApiError('Unexpected error')
}
