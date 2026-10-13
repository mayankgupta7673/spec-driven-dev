# 03 — Repository Structure

```
sdd-azure-functions-demo/
├── .github/
│   ├── copilot-instructions.md       # always-on rules for Copilot
│   ├── prompts/
│   │   ├── jira-to-spec.prompt.md     # inbound: JIRA issue → spec
│   │   ├── ado-to-spec.prompt.md      # inbound: ADO work item → spec
│   │   └── tasks-to-ado.prompt.md     # outbound: tasks → ADO work items
│   └── skills/speckit-*/             # spec-kit slash-command skills
├── .specify/
│   ├── memory/constitution.md         # the guardrails
│   ├── templates/                    # spec / plan / tasks / checklist templates
│   └── scripts/powershell/           # create-new-feature, setup-plan, setup-tasks
├── docs/
│   ├── architecture/                 # these documents
│   └── adr/                          # decision records
├── specs/
│   └── 001-order-submission/         # worked sample feature
│       ├── spec.md
│       ├── plan.md
│       ├── contracts/orders-api.md
│       └── tasks.md
├── src/SddDemo.Functions/
│   ├── Functions/                    # HTTP trigger adapters (thin)
│   ├── Models/                       # records: contracts, envelopes, value objects
│   ├── Services/                     # orchestration + swappable ports
│   ├── Validation/                   # FluentValidation validators
│   └── Program.cs                    # DI composition root
├── tests/SddDemo.Functions.Tests/    # xUnit, runs offline
├── demo.http                         # ready-to-run request samples
└── SddDemo.slnx
```

## Layering rules

| Folder | May reference | Must NOT reference |
|--------|---------------|--------------------|
| `Functions/` | `Services/`, `Models/`, `Validation/`, Azure Functions SDK | business rules inline |
| `Services/` | `Models/` | Azure Functions SDK, Azure client SDKs (use ports) |
| `Models/` | nothing | everything else |
| `Validation/` | `Models/`, `Services/` (for lookups) | Azure SDK |

**Litmus test:** `tests/SddDemo.Functions.Tests` compiles and runs with no Azure packages and no
network. If a change breaks that, the layering is wrong.

## The ports (swappable adapters)

- `IMessagePublisher` — `InMemoryMessagePublisher` (demo) ↔ Service Bus publisher (prod).
- `ISubmissionStatusStore` — `InMemorySubmissionStatusStore` (demo) ↔ Azure SQL/Dapper store (prod).
- `ILegalEntityMap` — `StaticLegalEntityMap` (demo) ↔ config/SQL-backed map (prod).

Swapping environments is a change to `Program.cs` only. See
[ADR-0004](../adr/ADR-0004-in-memory-adapters-for-demo.md).
