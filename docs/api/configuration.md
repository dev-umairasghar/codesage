# Configuration API

## `GET /api/v1/configuration`

Public, non-secret configuration summary for clients and troubleshooting.

```json
{
  "application": "CodeSage",
  "version": "0.1.0",
  "environment": "Development",
  "gitHubApiBaseUrl": "https://api.github.com/",
  "gitHubTokenConfigured": true,
  "aiProvider": "OpenAI",
  "aiModel": "gpt-4o-mini",
  "openAiBaseUrl": "https://api.openai.com/v1/",
  "openAiApiKeyConfigured": true,
  "probeExternalConnectivity": true,
  "requireSecretsAtStartup": true
}
```

Booleans indicate whether secrets are present — values are never returned.

For how to set secrets locally, see [Configuration.md](../Configuration.md).
