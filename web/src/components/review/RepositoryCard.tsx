import { Link } from 'react-router-dom'
import { GitBranch, Lock, Unlock } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Card, CardDescription, CardTitle } from '@/components/ui/card'
import { formatDate } from '@/lib/utils'
import type { RepositorySummary } from '@/types/api'

export function RepositoryCard({ repository }: { repository: RepositorySummary }) {
  const [owner, name] = repository.fullName.split('/')
  const to = `/repositories/${encodeURIComponent(owner ?? repository.ownerLogin)}/${encodeURIComponent(name ?? repository.name)}`

  return (
    <Link to={to} className="block no-underline transition hover:-translate-y-0.5">
      <Card className="h-full hover:border-accent/40">
        <div className="flex items-start justify-between gap-3">
          <div>
            <CardTitle className="font-mono text-sm text-accent">{repository.fullName}</CardTitle>
            <CardDescription className="line-clamp-2">
              {repository.description ?? 'No description'}
            </CardDescription>
          </div>
          <Badge className="shrink-0">
            {repository.private ? (
              <>
                <Lock className="mr-1 size-3" aria-hidden />
                Private
              </>
            ) : (
              <>
                <Unlock className="mr-1 size-3" aria-hidden />
                Public
              </>
            )}
          </Badge>
        </div>
        <div className="mt-4 flex flex-wrap gap-3 text-xs text-ink-muted">
          <span className="inline-flex items-center gap-1">
            <GitBranch className="size-3.5" aria-hidden />
            {repository.defaultBranch}
          </span>
          <span>Owner {repository.ownerLogin}</span>
          <span>Updated {formatDate(repository.updatedAt)}</span>
        </div>
      </Card>
    </Link>
  )
}
