import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { RepositoryCard } from '@/components/review/RepositoryCard'
import { EmptyState, ErrorMessage, LoadingSkeleton } from '@/components/shared/states'
import { Button } from '@/components/ui/button'
import { useRepositories } from '@/hooks/useApi'

const filterSchema = z.object({
  query: z.string().max(200),
})

type FilterValues = z.infer<typeof filterSchema>

export function RepositoriesPage() {
  const { data, isLoading, isError, error, refetch, isFetching } = useRepositories()
  const form = useForm<FilterValues>({
    resolver: zodResolver(filterSchema),
    defaultValues: { query: '' },
  })
  const [query, setQuery] = useState('')

  const filtered = useMemo(() => {
    const list = data ?? []
    const needle = query.trim().toLowerCase()
    if (!needle) {
      return list
    }
    return list.filter(
      (repo) =>
        repo.fullName.toLowerCase().includes(needle) ||
        repo.ownerLogin.toLowerCase().includes(needle) ||
        (repo.description?.toLowerCase().includes(needle) ?? false),
    )
  }, [data, query])

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold tracking-tight text-ink">Repositories</h1>
        <p className="max-w-2xl text-sm text-ink-muted">
          Browse repositories visible to your configured GitHub token, then open a repo to review
          pull requests.
        </p>
      </header>

      <form
        className="flex flex-col gap-2 sm:flex-row"
        onSubmit={form.handleSubmit((values) => setQuery(values.query))}
      >
        <input
          {...form.register('query')}
          placeholder="Filter by name, owner, or description"
          className="h-10 w-full rounded-md border border-border bg-panel px-3 text-sm text-ink placeholder:text-ink-subtle focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent"
        />
        <Button type="submit" variant="secondary">
          Filter
        </Button>
        <Button
          type="button"
          variant="ghost"
          disabled={isFetching}
          onClick={() => void refetch()}
        >
          Refresh
        </Button>
      </form>

      {isLoading ? <LoadingSkeleton rows={5} /> : null}
      {isError ? <ErrorMessage error={error} onRetry={() => void refetch()} /> : null}

      {!isLoading && !isError && filtered.length === 0 ? (
        <EmptyState
          title="No repositories found"
          description="Check your GitHub token scopes, or clear the filter."
        />
      ) : null}

      <div className="grid gap-3 sm:grid-cols-2">
        {filtered.map((repository) => (
          <RepositoryCard key={repository.id} repository={repository} />
        ))}
      </div>
    </div>
  )
}
