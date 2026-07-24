import { useId, useState, type ReactNode } from 'react'
import { ChevronDown } from 'lucide-react'
import { cn } from '@/lib/utils'

interface ExpandableSectionProps {
  title: string
  count?: number
  defaultOpen?: boolean
  children: ReactNode
}

export function ExpandableSection({
  title,
  count,
  defaultOpen = false,
  children,
}: ExpandableSectionProps) {
  const [open, setOpen] = useState(defaultOpen)
  const contentId = useId()

  return (
    <div className="overflow-hidden rounded-xl border border-border bg-panel/80">
      <button
        type="button"
        aria-expanded={open}
        aria-controls={contentId}
        className="flex w-full items-center justify-between gap-3 px-4 py-3 text-left hover:bg-panel-elevated/60"
        onClick={() => setOpen((value) => !value)}
      >
        <span className="flex items-center gap-2 text-sm font-medium text-ink">
          {title}
          {typeof count === 'number' ? (
            <span className="rounded-md bg-accent-soft px-1.5 py-0.5 font-mono text-[11px] text-accent">
              {count}
            </span>
          ) : null}
        </span>
        <ChevronDown
          className={cn('size-4 text-ink-muted transition', open && 'rotate-180')}
          aria-hidden
        />
      </button>
      {open ? (
        <div id={contentId} className="border-t border-border px-4 py-3">
          {children}
        </div>
      ) : null}
    </div>
  )
}
