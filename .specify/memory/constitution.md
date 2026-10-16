# SDD Azure Functions Demo Constitution

A gold-standard reference for Spec-Driven Development on .NET 10 isolated-worker
Azure Functions, with Azure DevOps work-item traceability. These principles are
the non-negotiable guardrails every `spec → plan → tasks → implement` cycle must honour.

## Core Principles

### I. Spec-First (NON-NEGOTIABLE)
No production code is written before a `spec.md` exists for the feature.
Every feature flows through `specify → clarify → plan → tasks → implement → converge`.
Each phase produces a Markdown artifact that feeds the next. A prompt is never a substitute
for a spec. If a change is not traceable to a spec, it does not merge.

### II. Thin Adapters, Rich Core
A Function trigger is an adapter (~30 lines): bind, validate, dispatch, return.
All real logic lives in `Services/`, `Packaging/`, and domain types — never in the trigger.
Inner folders (`Models/`, `Services/`, `Packaging/`) carry zero Azure SDK references.
Litmus test: the `*.Functions.Tests` project runs fully offline, no Azure required.

### III. Test-First & Testable Offline
Unit tests (xUnit + FluentAssertions + NSubstitute) accompany every domain change and must
run without Azure infrastructure. Test names follow `Should_X_When_Y`. No `Thread.Sleep`,
no random data, no `[Skip]` without a linked issue. Integration tests use Testcontainers.

### IV. Traceable to the Backlog (JIRA & Azure DevOps)
Requirements originate in **JIRA or Azure DevOps**. The originating **ticket id is the feature id**
(e.g. `proj-1234`, `ado-12345`) and is the single correlation key across ticket → spec → branch → PR.
Inbound: seed `spec.md` from the ticket (`jira-to-spec` / `ado-to-spec` prompts), enriching — never
silently overwriting — the source. Outbound: `tasks.md` produces child work items linked back under
the same ticket (`tasks-to-ado` prompt). Every PR references its ticket id. No work item or ticket
field is written without explicit human approval.

### V. Observable & Secure by Default
Structured `ILogger<T>` logging only — never string interpolation in templates.
A `{CorrelationId}` flows through every log scope and every outbound call.
Managed Identity for every PaaS-to-PaaS hop. No secrets in code or config.

## Technology Constraints

- **.NET 10 LTS, C# 14**, Azure Functions isolated worker model.
- **Dapper** for persistence — not EF Core. Purpose-named repositories, no generic `IRepository<T>`.
- **System.Text.Json** with source generators. Never Newtonsoft.Json in new code.
- **Polly v8** (`AddResilienceHandler`) for retry / circuit breaker / timeout.
- **FluentValidation** for inbound contract validation.
- `Nullable enable`, `TreatWarningsAsErrors=true`. Analyzers fail the build.
- Forbidden: `static` mutable state, `DateTime.UtcNow`/`Now` (inject `TimeProvider`),
  `new HttpClient()` (use `IHttpClientFactory` typed clients), service-locator patterns,
  "Helper"/"Manager"/"Util" classes, catching `Exception` to log-and-swallow.
- Required: `record` DTOs/envelopes, `sealed` classes by default, `CancellationToken` through
  every async signature, one class per file, `Async` suffix on async methods.

## Development Workflow

1. `/speckit.constitution` — once per project (this file).
2. **Seed from the backlog (inbound)** — run `jira-to-spec` or `ado-to-spec` to generate `spec.md`
   from the originating ticket, or run `/speckit.specify` directly for net-new work.
3. `/speckit.clarify` — resolve ambiguity before planning (optional but recommended).
4. `/speckit.plan` — `plan.md` + `contracts/` + `data-model.md` (tech decisions here).
5. `/speckit.tasks` — `tasks.md`, dependency-ordered. Then `tasks-to-ado` links child items back
   under the originating ticket (outbound).
6. `/speckit.analyze` — cross-artifact consistency gate (optional).
7. `/speckit.implement` — build the vertical slice with tests.
8. `/speckit.converge` — assess against artifacts, append remaining work. Repeat until Converged.

A vertical slice touches: one `{context}.Functions` (trigger/service/packaging),
one `{context}.Functions.Tests`, and optionally one APIM contract + one DB migration.

## Governance

This constitution supersedes ad-hoc practices. Every PR and review verifies compliance with
the principles above. Complexity must be justified in the plan's Complexity Tracking section.
Amendments require a version bump, dated entry below, and a note in the affected templates.

### Amendment history

- **1.1.0** (2026-10-16): Technology Constraints upgraded from .NET 8 LTS/C# 12 to .NET 10 LTS/C# 14
  (isolated worker). Deploy target: Azure Functions Consumption plan via GitHub Actions. No principle
  changed; `Models/`/`Services/`/`Validation/` remain Azure-SDK-free and offline-testable.

**Version**: 1.1.0 | **Ratified**: 2026-10-13 | **Last Amended**: 2026-10-16
