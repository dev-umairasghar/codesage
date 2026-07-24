import type { HTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

export function Badge({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-md border border-border bg-panel-elevated px-2 py-0.5 font-mono text-[11px] uppercase tracking-wide text-ink-muted',
        className,
      )}
      {...props}
    />
  )
}
