import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { pullRequestsApi } from '@/api/pullRequestsApi'
import { repositoriesApi } from '@/api/repositoriesApi'
import { reviewsApi } from '@/api/reviewsApi'
import { systemApi } from '@/api/systemApi'
import { loadSessionReviews, saveSessionReview } from '@/utils/sessionReviews'
import type { ReviewReport, StoredReview } from '@/types/api'

export const queryKeys = {
  repositories: ['repositories'] as const,
  repository: (owner: string, name: string) => ['repository', owner, name] as const,
  pullRequests: (owner: string, name: string) => ['pull-requests', owner, name] as const,
  pullRequest: (owner: string, name: string, number: number) =>
    ['pull-request', owner, name, number] as const,
  systemStatus: ['system-status'] as const,
  configuration: ['configuration'] as const,
  sessionReviews: ['session-reviews'] as const,
}

export function useRepositories() {
  return useQuery({
    queryKey: queryKeys.repositories,
    queryFn: () => repositoriesApi.list(),
  })
}

export function useRepository(owner: string, name: string) {
  return useQuery({
    queryKey: queryKeys.repository(owner, name),
    queryFn: () => repositoriesApi.get(owner, name),
    enabled: Boolean(owner && name),
  })
}

export function usePullRequests(owner: string, name: string) {
  return useQuery({
    queryKey: queryKeys.pullRequests(owner, name),
    queryFn: () => pullRequestsApi.list(owner, name),
    enabled: Boolean(owner && name),
  })
}

export function usePullRequest(owner: string, name: string, number: number) {
  return useQuery({
    queryKey: queryKeys.pullRequest(owner, name, number),
    queryFn: () => pullRequestsApi.get(owner, name, number),
    enabled: Boolean(owner && name && number > 0),
  })
}

export function useSystemStatus() {
  return useQuery({
    queryKey: queryKeys.systemStatus,
    queryFn: () => systemApi.status(),
  })
}

export function useConfiguration() {
  return useQuery({
    queryKey: queryKeys.configuration,
    queryFn: () => systemApi.configuration(),
  })
}

export function useSessionReviews() {
  return useQuery({
    queryKey: queryKeys.sessionReviews,
    queryFn: () => loadSessionReviews(),
    staleTime: Infinity,
  })
}

export interface RunReviewInput {
  owner: string
  repo: string
  number: number
  pullRequestTitle: string
  authorLogin: string
}

export function useRunReview() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (input: RunReviewInput): Promise<{ report: ReviewReport; entry: StoredReview }> => {
      const context = await reviewsApi.analyze(input.owner, input.repo, input.number)
      const report = await reviewsApi.create(context)
      const entry: StoredReview = {
        id: crypto.randomUUID(),
        createdAt: new Date().toISOString(),
        owner: input.owner,
        repo: input.repo,
        pullRequestNumber: input.number,
        pullRequestTitle: input.pullRequestTitle,
        authorLogin: input.authorLogin,
        report,
      }
      saveSessionReview(entry)
      return { report, entry }
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.sessionReviews })
    },
  })
}
