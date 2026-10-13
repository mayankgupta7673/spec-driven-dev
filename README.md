# SDD Azure Functions Demo

A gold-standard reference showing **Spec-Driven Development (SDD)** on **.NET 8 isolated-worker
Azure Functions**, wired for **GitHub Copilot** and **Azure DevOps** traceability.

## The idea

Every feature is built by running the spec-kit loop — never by ad-hoc prompting:

```
/speckit.constitution   (once per project)
/speckit.specify        → specs/<feature>/spec.md
/speckit.clarify        → de-risk ambiguity
/speckit.plan           → plan.md + contracts/ + data-model.md
/speckit.tasks          → tasks.md (dependency-ordered)
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
| `.github/prompts/tasks-to-ado.prompt.md` | Map `tasks.md` → Azure DevOps Epic/Feature/Story |
| `.github/skills/speckit-*/` | The spec-kit slash-command skills |
| `specs/001-order-submission/` | Worked sample spec (async purchase-order submission) |
| `src/SddDemo.Functions/` | .NET 8 isolated Functions app (thin adapters) |
| `tests/SddDemo.Functions.Tests/` | xUnit + FluentAssertions + NSubstitute (runs offline) |

## Build & test

```powershell
dotnet build SddDemo.slnx
dotnet test SddDemo.slnx
```

## Next step

Open `specs/001-order-submission/spec.md`, then run `/speckit.plan` in Copilot Chat to generate
the implementation plan, followed by `/speckit.tasks` and `/speckit.implement`.
