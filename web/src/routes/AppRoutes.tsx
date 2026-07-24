import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from '@/layouts/AppLayout'
import { AboutPage } from '@/pages/AboutPage'
import { ConfigurationPage } from '@/pages/ConfigurationPage'
import { PullRequestsPage } from '@/pages/PullRequestsPage'
import { RepositoriesPage } from '@/pages/RepositoriesPage'
import { ReviewPage } from '@/pages/ReviewPage'
import { ReviewsPage } from '@/pages/ReviewsPage'

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Navigate to="/repositories" replace />} />
        <Route path="repositories" element={<RepositoriesPage />} />
        <Route path="repositories/:owner/:repo" element={<PullRequestsPage />} />
        <Route
          path="repositories/:owner/:repo/pull-requests/:number"
          element={<ReviewPage />}
        />
        <Route path="reviews" element={<ReviewsPage />} />
        <Route path="configuration" element={<ConfigurationPage />} />
        <Route path="about" element={<AboutPage />} />
        <Route path="*" element={<Navigate to="/repositories" replace />} />
      </Route>
    </Routes>
  )
}
