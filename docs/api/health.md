# Health API

## `GET /api/v1/health`

Liveness probe. Does not call GitHub or OpenAI.

Also available as unversioned `GET /api/health`.

```json
{
  "status": "Healthy",
  "application": "CodeSage",
  "version": "0.1.0"
}
```

## `GET /api/v1/system/status`

Diagnostics for local installs: whether secrets are configured, optional connectivity probes, friendly messages.

Never returns API keys or tokens.

Disable probes with `Application:ProbeExternalConnectivity=false`.
