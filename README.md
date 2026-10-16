# SDD Azure Functions Demo

A gold-standard reference showing **Spec-Driven Development (SDD)** on **.NET 10 isolated-worker
Azure Functions**, wired for **GitHub Copilot** and **JIRA + Azure DevOps** traceability.

## The idea

Requirements originate in **JIRA or Azure DevOps**; the **ticket id is the feature id** and the
correlation key across ticket → spec → branch → PR. Every feature is built by running the loop —
never by ad-hoc prompting:

```
/speckit.constitution   (once per project)
jira-to-spec | ado-to-spec  → seed specs/<feature>/spec.md from the ticket (inbound)
/speckit.clarify        → de-risk ambiguity
/speckit.plan           → plan.md + contracts/ + data-model.md
/speckit.tasks          → tasks.md (dependency-ordered)
tasks-to-ado            → create child work items under the ticket (outbound)
/speckit.analyze        → cross-artifact consistency gate
/speckit.implement      → the vertical slice + tests
/speckit.converge       → assess vs artifacts, append remaining work
```

## Layout

| Path | Purpose |
|------|---------|
| `.specify/memory/constitution.md` | Project principles — the non-negotiable guardrails |
| `.specify/templates/` | spec / plan / tasks / checklist templates |
| `.github/copilot-instructions.md` | Always-on rules for Copilot (mirrors the constitution) |
| `.github/prompts/jira-to-spec.prompt.md` | Seed `spec.md` from a JIRA issue (inbound) |
| `.github/prompts/ado-to-spec.prompt.md` | Seed `spec.md` from an ADO work item (inbound) |
| `.github/prompts/tasks-to-ado.prompt.md` | Map `tasks.md` → ADO Epic/Feature/Story (outbound) |
| `.github/skills/speckit-*/` | The spec-kit slash-command skills |
| `specs/001-order-submission/` | Worked sample spec (async purchase-order submission) |
| `src/SddDemo.Functions/` | .NET 10 isolated Functions app (thin adapters) |
| `tests/SddDemo.Functions.Tests/` | xUnit + FluentAssertions + NSubstitute (runs offline) |
| `docs/` | Architecture docs + decision records ([index](docs/README.md)) |
| `CONTRIBUTING.md` | How to add a feature + the vertical-slice checklist |

## Build & test

```powershell
dotnet build SddDemo.slnx
dotnet test SddDemo.slnx
```

## Run the demo (offline, no Azure required)

```powershell
cd src/SddDemo.Functions
func start
```

Then exercise the endpoints with [demo.http](demo.http) (REST Client extension) or PowerShell:

```powershell
# Submit an order -> 202 { correlationId }
$body = '{"legalEntityId":"ABN-01","supplier":"ACME Pty Ltd","clientRequestId":"req-1","lines":[{"sku":"TIMBER-90X45","quantity":120,"unitPrice":7.85}]}'
$r = Invoke-RestMethod http://localhost:7071/api/orders -Method Post -ContentType application/json -Body $body
$r.correlationId

# Query status -> 200 { state: "Accepted" }
Invoke-RestMethod "http://localhost:7071/api/orders/$($r.correlationId)/status"
```

Demo talking points:
- **202 + correlationId** — caller is decoupled from the slow D365 write.
- **Idempotency** — resubmitting the same `clientRequestId` returns the original id, publishes once.
- **Boundary validation** — missing lines or an unknown legal entity returns `400`, publishes nothing.
- **Swap to Azure** — replace two lines in `Program.cs` (in-memory publisher/store → Service Bus + Azure SQL); Functions, services, and models are untouched.

## Next step

Open `specs/001-order-submission/spec.md`, then run `/speckit.plan` in Copilot Chat to generate
the implementation plan, followed by `/speckit.tasks` and `/speckit.implement`.
