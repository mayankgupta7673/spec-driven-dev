# Documentation

Authoritative docs for the **SDD Azure Functions Demo** — a gold-standard reference for
Spec-Driven Development on .NET 10 isolated-worker Azure Functions with JIRA + Azure DevOps
traceability.

> If anything here conflicts with [.specify/memory/constitution.md](../.specify/memory/constitution.md),
> the constitution wins.

## Architecture

| Doc | Purpose |
|-----|---------|
| [architecture/01-overview.md](architecture/01-overview.md) | What the system is and its guiding principles |
| [architecture/02-sdd-workflow.md](architecture/02-sdd-workflow.md) | The spec → plan → tasks → implement → converge loop |
| [architecture/03-repository-structure.md](architecture/03-repository-structure.md) | Folder layout and the vertical-slice rule |
| [architecture/04-submission-pipeline.md](architecture/04-submission-pipeline.md) | How an order flows from HTTP to the submit transport |
| [architecture/05-traceability.md](architecture/05-traceability.md) | JIRA/ADO inbound and outbound traceability |

## Decision records

| ADR | Decision |
|-----|----------|
| [ADR-0001](adr/ADR-0001-spec-driven-development.md) | Spec-Driven Development as the delivery method |
| [ADR-0002](adr/ADR-0002-thin-adapter-functions.md) | Function triggers are thin adapters |
| [ADR-0003](adr/ADR-0003-dapper-not-ef-core.md) | Dapper for persistence, not EF Core |
| [ADR-0004](adr/ADR-0004-in-memory-adapters-for-demo.md) | In-memory adapters behind interfaces for offline demo |
| [ADR-0005](adr/ADR-0005-ticket-id-as-feature-id.md) | The backlog ticket id is the feature correlation id |

## Conventions

See [../CONTRIBUTING.md](../CONTRIBUTING.md) for how to add a feature and the vertical-slice checklist.

## Presentation

A self-contained slide deck explaining Spec-Driven Development (open in any browser, works offline):
[presentation/spec-driven-development.html](presentation/spec-driven-development.html).
Navigate with ← → / Space; press F for fullscreen.
