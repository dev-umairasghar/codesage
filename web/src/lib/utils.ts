import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatDate(value: string | Date) {
  const date = typeof value === 'string' ? new Date(value) : value
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}

export function riskToScore(risk: string): number {
  switch (risk) {
    case 'Low':
      return 90
    case 'Medium':
      return 70
    case 'High':
      return 40
    case 'Critical':
      return 15
    default:
      return 50
  }
}

export function enumLabel(value: string | number | undefined | null): string {
  if (value === null || value === undefined) {
    return 'Unknown'
  }
  if (typeof value === 'number') {
    return String(value)
  }
  return value
}
