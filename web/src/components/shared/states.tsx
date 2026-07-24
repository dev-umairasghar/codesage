import { AlertCircle, Inbox, Loader2, RefreshCw } from 'lucide-react'
import type { ReactNode } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ApiError } from '@/api/http'

export function LoadingState({ label = 'Loading…' }: { label?: string }) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-border bg-panel/70 px-4 py-6 text-sm text-ink-muted">
      <Loader2 className="size-4 animate-spin text-accent" aria-hidden />
      {label}
    </div>
  )
}

export function LoadingSkeleton({ rows = 4 }: { rows?: number }) {
  return (
    <div className="space-y-3" data-testid="loading-skeleton">
      {Array.from({ length: rows }).map((_, index) => (
        <Skeleton key={index} className="h-20 w-full" />
      ))}
    </div>
  )
}

export function EmptyState({
  title,
  description,
  action,
}: {
  title: string
  description?: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col items-start gap-3 rounded-xl border border-dashed border-border-strong bg-panel/50 px-5 py-8">
      <Inbox className="size-5 text-ink-subtle" aria-hidden />
      <div>
        <p className="text-sm font-medium text-ink">{title}</p>
        {description ? <p className="mt-1 text-sm text-ink-muted">{description}</p> : null}
      </div>
      {action}
    </div>
  )
}

export function ErrorMessage({
  error,
  onRetry,
}: {
  error: unknown
  onRetry?: () => void
}) {
  const apiError = error instanceof ApiError ? error : null
  const message =
    apiError?.message ?? (error instanceof Error ? error.message : 'Something went wrong')

  return (
    <div
      role="alert"
      className="flex flex-col gap-3 rounded-xl border border-danger/30 bg-danger/10 px-4 py-4"
    >
      <div className="flex items-start gap-2">
        <AlertCircle className="mt-0.5 size-4 shrink-0 text-danger" aria-hidden />
        <div>
          <p className="text-sm font-medium text-ink">{apiError?.problem?.title ?? 'Request failed'}</p>
          <p className="mt-1 text-sm text-ink-muted">{message}</p>
          {apiError?.errorCode ? (
            <p className="mt-2 font-mono text-[11px] text-ink-subtle">code: {apiError.errorCode}</p>
          ) : null}
        </div>
      </div>
      {onRetry ? <RetryButton onClick={onRetry} /> : null}
    </div>
  )
}

export function RetryButton({ onClick }: { onClick: () => void }) {
  return (
    <Button type="button" variant="secondary" size="sm" onClick={onClick}>
      <RefreshCw className="size-3.5" aria-hidden />
      Retry
    </Button>
  )
}
