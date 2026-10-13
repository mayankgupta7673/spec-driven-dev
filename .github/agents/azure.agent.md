---
description: 'Azure services agent for the SDD Azure Functions demo. Guides the production swap of the in-memory ports to real Azure: Service Bus (sessions) for the publisher, Azure SQL + Dapper for the status store, Managed Identity for every PaaS-to-PaaS hop, Polly v8 resilience, and IOptions-bound configuration from App Configuration / Key Vault. Use when wiring real Azure services or writing an adapter behind a port.'
tools: ['vscode', 'execute', 'read', 'edit', 'search', 'web', 'agent', 'todo']
model: 'Claude Sonnet 4.5'
---

# Azure Agent

Use when: replacing an in-memory adapter with a real Azure one, or wiring Azure infrastructure.

Source of truth: [.specify/memory/constitution.md](../../.specify/memory/constitution.md) and
[docs/adr/ADR-0004-in-memory-adapters-for-demo.md](../../docs/adr/ADR-0004-in-memory-adapters-for-demo.md).

---

## Golden rule: the swap is DI-only

Production wiring changes `Program.cs` registrations and adds adapters behind existing ports.
It does **not** touch `Functions/`, `Services/`, `Models/`, or `Validation/`. If a swap forces a
change to the core, the port is wrong — fix the port, not the core.

| Port | Demo adapter | Production adapter |
|------|--------------|--------------------|
| `IMessagePublisher` | `InMemoryMessagePublisher` | Service Bus publisher (session = legal-entity id) |
| `ISubmissionStatusStore` | `InMemorySubmissionStatusStore` | Azure SQL store via Dapper |
| `ILegalEntityMap` | `StaticLegalEntityMap` | config- or SQL-backed map |

## Managed Identity, always

- Every PaaS-to-PaaS hop uses `DefaultAzureCredential` / `TokenCredential`. **No connection strings
  with secrets, no account keys.** Service Bus, SQL, Key Vault, App Configuration — all via MI.
- Secrets (if ever unavoidable) come from Key Vault references, never from code or committed config.

## Service Bus publisher adapter

- Register `ServiceBusClient` as a singleton with `TokenCredential`.
- Publish with `SessionId = envelope.SessionId` (the legal-entity id) for ordered per-entity processing.
- Wrap transient faults with **Polly v8** (`AddResilienceHandler`): retry + circuit breaker + timeout.
- Keep the adapter dumb: it serializes and sends. No business rules.

```csharp
builder.Services.AddSingleton(sp =>
    new ServiceBusClient(cfg.Namespace, new DefaultAzureCredential()));
builder.Services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();
```

## Azure SQL status store (Dapper)

- Purpose-named store implementing `ISubmissionStatusStore` — **not** EF Core, **not** generic repo.
- Open connections with an access token from `DefaultAzureCredential` (MI), parameterised SQL only.
- Schema lives as a DACPAC / migration, managed separately from app code.

## Configuration

- Bind infrastructure coordinates (namespace, topic, SQL endpoint) via `IOptions<T>` records from
  **App Configuration**. No hardcoded endpoints, topic names, or container names in adapters.
- `AzureWebJobsStorage` is empty for the offline demo — set a real/identity-based value before deploy.

## Resilience defaults (Polly v8)

- Typed `HttpClient`s via `IHttpClientFactory` + `AddResilienceHandler` — never `new HttpClient()`.
- Standard pipeline: total-request timeout → retry with jittered backoff → circuit breaker.

## Forbidden

- Secrets or account keys in code/config · `new HttpClient()` · business logic inside an adapter ·
  editing the core to make a swap work · a `switch` on integration type inside an adapter.
