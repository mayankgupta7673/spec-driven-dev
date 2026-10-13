# Copilot Instructions – SDD Azure Functions Demo

Authoritative guardrails: [.specify/memory/constitution.md](../.specify/memory/constitution.md).
If anything here conflicts with the constitution, the constitution wins.

## What this project is

A gold-standard reference for **Spec-Driven Development (SDD)** on **.NET 8 isolated-worker
Azure Functions**, with **Azure DevOps** work-item traceability. Every feature is built by
running the spec-kit loop, not by ad-hoc prompting.

## The loop (do this for every feature)

`/speckit.constitution` (once) → `/speckit.specify` → `/speckit.clarify` → `/speckit.plan`
→ `/speckit.tasks` → `/speckit.analyze` → `/speckit.implement` → `/speckit.converge`

Never write production code before a `spec.md` exists for the feature.

## Stack (do not deviate without updating the constitution)

- .NET 8 LTS, C# 12, Azure Functions **isolated worker**.
- **Dapper** for persistence — not EF Core.
- **System.Text.Json** source generators — never Newtonsoft.Json.
- **Polly v8** `AddResilienceHandler` for retry / circuit breaker / timeout.
- **FluentValidation** for inbound contracts.
- **xUnit + FluentAssertions + NSubstitute**; Testcontainers for integration.

## Non-negotiables

- `Nullable enable`, `TreatWarningsAsErrors=true`.
- **Function trigger = adapter** (~30 lines): bind, validate, dispatch, return. Logic lives in `Services/` and `Packaging/`.
- Inner folders (`Models/`, `Services/`, `Packaging/`) carry **zero Azure SDK references**; `*.Functions.Tests` runs offline.
- `{CorrelationId}` in every log scope and outbound call.
- **Managed Identity** for every PaaS-to-PaaS hop. No secrets in code or config.

## Forbidden patterns (do not generate)

- `static` mutable state.
- `DateTime.UtcNow` / `DateTime.Now` — inject `TimeProvider`.
- `new HttpClient()` — use `IHttpClientFactory` typed clients.
- Service-locator (`IServiceProvider.GetService<T>()` in business code).
- Generic `IRepository<T>` — write a purpose-named repository per aggregate.
- "Helper" / "Manager" / "Util" classes.
- Catching `Exception` to log-and-swallow.

## Required patterns

- `record` for DTOs / envelopes / value objects.
- `sealed` by default; `CancellationToken` through every async signature.
- Structured logging only: `log.LogInformation("Submitted {CorrelationId}", id)`.
- One class per file; `Async` suffix on async methods; `I` prefix on interfaces; `Should_X_When_Y` tests.

## Traceability to Azure DevOps

After `/speckit.tasks`, map each task to an ADO work item (Epic → Feature → Story) using the
prompt in [.github/prompts/tasks-to-ado.prompt.md](prompts/tasks-to-ado.prompt.md).
Never create ADO work items without explicit human approval. Every PR references its Story id.
