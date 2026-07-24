export type ReviewRiskLevel = 'Unknown' | 'Low' | 'Medium' | 'High' | 'Critical'

export type FindingSeverity = 'Unknown' | 'Info' | 'Low' | 'Medium' | 'High' | 'Critical'

export type FindingCategory =
  | 'Unknown'
  | 'CodeQuality'
  | 'Maintainability'
  | 'Readability'
  | 'Architecture'
  | 'Performance'
  | 'Security'
  | 'ErrorHandling'
  | 'Naming'
  | 'BugRisk'
  | 'MissingTests'
  | 'RegressionRisk'
  | 'Other'

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errorCode?: string
  errors?: Record<string, string[]>
}

export interface RepositorySummary {
  id: number
  name: string
  fullName: string
  ownerLogin: string
  description: string | null
  private: boolean
  htmlUrl: string
  defaultBranch: string
  updatedAt: string
}

export interface RepositoryDetails extends RepositorySummary {
  language: string | null
  openIssuesCount: number
  forksCount: number
  stargazersCount: number
  createdAt: string
}

export interface PullRequestSummary {
  number: number
  title: string
  state: string
  draft: boolean
  authorLogin: string
  authorAvatarUrl: string | null
  createdAt: string
  updatedAt: string
  htmlUrl: string
}

export interface ChangedFile {
  filename: string
  status: string
  additions: number
  deletions: number
  changes: number
  patch: string | null
}

export interface CommitSummary {
  sha: string
  message: string
  authorName: string
  authorLogin: string | null
  committedAt: string | null
}

export interface PullRequestComment {
  id: number
  authorLogin: string
  body: string
  createdAt: string
  kind: string
  path: string | null
  line: number | null
}

export interface PullRequestDetails extends PullRequestSummary {
  description: string | null
  baseRef: string
  headRef: string
  changedFiles: ChangedFile[]
  commits: CommitSummary[]
  comments: PullRequestComment[]
}

export interface ReviewFinding {
  title: string
  description: string
  whyItMatters: string
  severity: FindingSeverity | number
  category: FindingCategory | number
  filePath: string | null
  startLine: number | null
  endLine: number | null
  suggestion: string | null
}

export interface ReviewReport {
  summary: string
  overallRisk: ReviewRiskLevel | number
  positiveFindings: string[]
  issues: ReviewFinding[]
  recommendations: string[]
  missingTests: string[]
  securityConcerns: ReviewFinding[]
  performanceConcerns: ReviewFinding[]
  maintainability: ReviewFinding[]
  architectureConcerns: ReviewFinding[]
  model: string
  promptTokens: number | null
  completionTokens: number | null
  totalTokens: number | null
  duration: string
}

/** Opaque ReviewContext from analysis — forwarded to POST /reviews. */
export type ReviewContext = Record<string, unknown>

export interface ConnectivityCheck {
  status: string
  message: string
  httpStatusCode: number | null
}

export interface SystemStatus {
  application: string
  version: string
  environment: string
  aiProvider: string
  aiModel: string
  gitHubTokenConfigured: boolean
  openAiApiKeyConfigured: boolean
  gitHubConnectivity: ConnectivityCheck
  openAiConnectivity: ConnectivityCheck
  diagnostics: string[]
}

export interface ConfigurationSummary {
  application: string
  version: string
  environment: string
  gitHubApiBaseUrl: string
  gitHubTokenConfigured: boolean
  aiProvider: string
  aiModel: string
  openAiBaseUrl: string
  openAiApiKeyConfigured: boolean
  probeExternalConnectivity: boolean
  requireSecretsAtStartup: boolean
}

export interface HealthResponse {
  status: string
  application: string
  version: string
}

/** Client-side session history entry (API is stateless). */
export interface StoredReview {
  id: string
  createdAt: string
  owner: string
  repo: string
  pullRequestNumber: number
  pullRequestTitle: string
  authorLogin: string
  report: ReviewReport
}
