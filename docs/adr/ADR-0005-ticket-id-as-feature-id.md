# ADR-0005 — The backlog ticket id is the feature correlation id

**Status**: Accepted · **Date**: 2026-10-13

## Context

Requirements arrive from **both JIRA and Azure DevOps**. Teams need to trace a line of work from the
originating ticket all the way to the merged PR without manual bookkeeping.

## Decision

Adopt a **hybrid** model where the originating **ticket id is the feature id**
(e.g. `proj-1234`, `ado-12345`) and the single correlation key across
ticket → spec → branch → PR → child work items.

- **Inbound**: `jira-to-spec` / `ado-to-spec` prompts read the ticket and seed `spec.md`, carrying
  the id into the feature short name and the `**Input**` line. Inbound **enriches**; it never
  silently overwrites the source.
- **Outbound**: `tasks-to-ado` creates child work items linked back under the same ticket.
- The feature id is used as the branch prefix and referenced in the PR.

## Consequences

- **Positive**: end-to-end traceability by a single id; works for either tool; no duplicate systems
  of record.
- **Negative**: requires access to the backlog tool (ADO MCP, or a JIRA MCP / REST token); write-back
  needs explicit approval and secret handling discipline.
- Secrets are read from the environment only, never printed or committed.

See [architecture/05-traceability.md](../architecture/05-traceability.md) for the field mapping.
