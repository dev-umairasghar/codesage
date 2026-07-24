import { NavLink, Outlet } from 'react-router-dom'
import { BookOpen, FolderGit2, Settings2, Sparkles } from 'lucide-react'
import { cn } from '@/lib/utils'

const navItems = [
  { to: '/repositories', label: 'Repositories', icon: FolderGit2 },
  { to: '/reviews', label: 'Reviews', icon: Sparkles },
  { to: '/configuration', label: 'Configuration', icon: Settings2 },
  { to: '/about', label: 'About', icon: BookOpen },
] as const

export function AppLayout() {
  return (
    <div className="flex min-h-svh flex-col">
      <header className="sticky top-0 z-20 border-b border-border/80 bg-canvas/85 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
          <NavLink to="/" className="flex items-center gap-2 no-underline hover:no-underline">
            <span className="flex size-8 items-center justify-center rounded-lg bg-accent-soft font-mono text-sm font-bold text-accent">
              CS
            </span>
            <span className="text-lg font-semibold tracking-tight text-ink">CodeSage</span>
          </NavLink>
          <p className="hidden text-xs text-ink-subtle sm:block">Local AI pull request review</p>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-7xl flex-1 gap-0 md:gap-6 px-0 sm:px-6">
        <aside className="hidden w-56 shrink-0 border-r border-border/60 py-6 md:block">
          <nav className="sticky top-20 space-y-1 pr-4">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-2 rounded-lg px-3 py-2 text-sm no-underline transition hover:no-underline',
                    isActive
                      ? 'bg-accent-soft text-accent'
                      : 'text-ink-muted hover:bg-panel-elevated hover:text-ink',
                  )
                }
              >
                <item.icon className="size-4" aria-hidden />
                {item.label}
              </NavLink>
            ))}
          </nav>
        </aside>

        <div className="flex min-w-0 flex-1 flex-col">
          <nav className="flex gap-1 overflow-x-auto border-b border-border/60 px-4 py-2 md:hidden">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  cn(
                    'whitespace-nowrap rounded-md px-3 py-1.5 text-xs no-underline hover:no-underline',
                    isActive ? 'bg-accent-soft text-accent' : 'text-ink-muted',
                  )
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
          <main className="flex-1 px-4 py-6 sm:px-0 sm:py-8">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
