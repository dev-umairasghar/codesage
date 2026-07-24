import { ErrorMessage, LoadingSkeleton } from '@/components/shared/states'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import { useConfiguration, useSystemStatus } from '@/hooks/useApi'
import { cn } from '@/lib/utils'

function StatusPill({ ok, label }: { ok: boolean; label: string }) {
  return (
    <Badge
      className={cn(
        ok ? 'border-success/40 text-success' : 'border-danger/40 text-danger',
      )}
    >
      {label}
    </Badge>
  )
}

export function ConfigurationPage() {
  const status = useSystemStatus()
  const configuration = useConfiguration()

  const isLoading = status.isLoading || configuration.isLoading
  const error = status.error ?? configuration.error

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold text-ink">Configuration</h1>
        <p className="max-w-2xl text-sm text-ink-muted">
          Read-only diagnostics from the CodeSage API. Secrets are never shown — only whether they
          are configured.
        </p>
      </header>

      <div className="flex gap-2">
        <Button
          type="button"
          variant="secondary"
          onClick={() => {
            void status.refetch()
            void configuration.refetch()
          }}
        >
          Refresh
        </Button>
      </div>

      {isLoading ? <LoadingSkeleton rows={3} /> : null}
      {error ? (
        <ErrorMessage
          error={error}
          onRetry={() => {
            void status.refetch()
            void configuration.refetch()
          }}
        />
      ) : null}

      {status.data && configuration.data ? (
        <div className="grid gap-3 md:grid-cols-2">
          <Card>
            <CardTitle>Application</CardTitle>
            <CardDescription className="mt-3 space-y-2 text-sm text-ink-muted">
              <div>Name: {status.data.application}</div>
              <div>Version: {status.data.version}</div>
              <div>Environment: {status.data.environment}</div>
            </CardDescription>
          </Card>

          <Card>
            <CardTitle>AI provider</CardTitle>
            <CardDescription className="mt-3 space-y-2 text-sm text-ink-muted">
              <div>Provider: {status.data.aiProvider}</div>
              <div>Model: {status.data.aiModel}</div>
              <div className="flex items-center gap-2">
                API key{' '}
                <StatusPill
                  ok={status.data.openAiApiKeyConfigured}
                  label={status.data.openAiApiKeyConfigured ? 'Configured' : 'Missing'}
                />
              </div>
              <div>
                Connectivity: {status.data.openAiConnectivity.status} —{' '}
                {status.data.openAiConnectivity.message}
              </div>
            </CardDescription>
          </Card>

          <Card>
            <CardTitle>GitHub</CardTitle>
            <CardDescription className="mt-3 space-y-2 text-sm text-ink-muted">
              <div>API: {configuration.data.gitHubApiBaseUrl}</div>
              <div className="flex items-center gap-2">
                Token{' '}
                <StatusPill
                  ok={status.data.gitHubTokenConfigured}
                  label={status.data.gitHubTokenConfigured ? 'Configured' : 'Missing'}
                />
              </div>
              <div>
                Connectivity: {status.data.gitHubConnectivity.status} —{' '}
                {status.data.gitHubConnectivity.message}
              </div>
            </CardDescription>
          </Card>

          <Card>
            <CardTitle>Diagnostics</CardTitle>
            <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-ink-muted">
              {status.data.diagnostics.map((item) => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          </Card>
        </div>
      ) : null}
    </div>
  )
}
