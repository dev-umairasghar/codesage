import type { StoredReview } from '@/types/api'

const STORAGE_KEY = 'codesage.session-reviews'

export function loadSessionReviews(): StoredReview[] {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    if (!raw) {
      return []
    }
    const parsed = JSON.parse(raw) as StoredReview[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

export function saveSessionReview(review: StoredReview): StoredReview[] {
  const next = [review, ...loadSessionReviews().filter((item) => item.id !== review.id)]
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next))
  return next
}

export function clearSessionReviews(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
