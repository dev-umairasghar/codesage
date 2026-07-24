# Configuration

CodeSage is a **local-first** tool. Configure it with the .NET configuration stack — never commit secrets.

## Precedence (highest wins)

1. Environment variables
2. User secrets (Development)
3. `appsettings.{Environment}.json`
4. `appsettings.json`

## Recommended for local development

**Use .NET User Secrets.** They stay outside the repo and work with `dotnet run`.

```bash
cd src/CodeSage.Api

dotnet user-secrets set "GitHub:PersonalAccessToken" "ghp_xxxxxxxx"
dotnet user-secrets set "OpenAI:ApiKey" "sk-xxxxxxxx"
```

Optional overrides:

```bash
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini"
dotnet user-secrets set "OpenAI:Temperature" "0.2"
dotnet user-secrets set "OpenAI:MaxTokens" "4096"
```

List current secrets (values are not shown by default tooling in some versions — treat the store as sensitive):

```bash
dotnet user-secrets list
```

## Option A — `appsettings.Development.json`

Useful for non-secret defaults (model, temperature, URLs). **Do not put API keys here if the file is committed.**

Example (safe defaults only):

```json
{
  "Application": {
    "ProbeExternalConnectivity": true
  },
  "GitHub": {
    "ApiBaseUrl": "https://api.github.com/",
    "UserAgent": "CodeSage"
  },
  "OpenAI": {
    "Model": "gpt-4o-mini",
    "Temperature": 0.2,
    "MaxTokens": 4096,
    "TimeoutSeconds": 120
  }
}
```

For a private machine-only override that is gitignored, you can use `appsettings.Development.local.json` patterns — prefer user-secrets instead.

## Option B — Environment variables

Nested keys use `__`:

```bash
export GitHub__PersonalAccessToken="ghp_xxxxxxxx"
export OpenAI__ApiKey="sk-xxxxxxxx"
export OpenAI__Model="gpt-4o-mini"
export OpenAI__Temperature="0.2"
export OpenAI__MaxTokens="4096"
export Application__RequireSecretsAtStartup="true"
```

Ideal for CI containers and shell profiles.

## Configuration sections

### `Application`

| Key | Default | Description |
|-----|---------|-------------|
| `Name` | `CodeSage` | Product name in health responses |
| `Version` | `0.1.0` | Version string |
| `Environment` | _(empty)_ | Override host environment label in diagnostics |
| `RequireSecretsAtStartup` | `true` | Fail host start if GitHub token / OpenAI key missing |
| `ProbeExternalConnectivity` | `true` | Allow `/api/system/status` to ping GitHub/OpenAI |

### `GitHub`

| Key | Description |
|-----|-------------|
| `PersonalAccessToken` | PAT with access to the repos you want to review |
| `ApiBaseUrl` | Default `https://api.github.com/` |
| `UserAgent` | Default `CodeSage` (required by GitHub) |

### `OpenAI`

| Key | Description |
|-----|-------------|
| `ApiKey` | Secret API key |
| `BaseUrl` | Default `https://api.openai.com/v1/` |
| `Model` | e.g. `gpt-4o-mini` |
| `Temperature` | `0`–`2` |
| `MaxTokens` | Must be &gt; 0 |
| `TimeoutSeconds` | `1`–`600` |

### `AI`

Prompt/logging knobs for the review engine (`LogPrompts`, patch size limits).

## Startup validation

On start, CodeSage validates options and logs (never secrets):

- Application starting
- Configuration loaded
- Configuration validation succeeded
- Registered services
- Application ready

Typical failure messages:

- Missing GitHub token → set `GitHub:PersonalAccessToken`
- Missing OpenAI key → set `OpenAI:ApiKey`
- Invalid temperature / max tokens / URLs → fix the numeric or URL values

Set `Application:RequireSecretsAtStartup` to `false` only for lightweight probes where GitHub/OpenAI calls are not needed (e.g. some CI health checks).

## Diagnostics

```http
GET /api/health
GET /api/system/status
```

`/api/system/status` reports whether secrets are **configured** (boolean), model name, environment, and optional connectivity — **never** the raw keys.
