# ADR-0003 — Dapper for persistence, not EF Core

**Status**: Accepted · **Date**: 2026-10-13

## Context

The production status store is Azure SQL. We need predictable SQL, low overhead in short-lived
Function invocations, and purpose-built queries rather than a general-purpose ORM.

## Decision

Use **Dapper** with **purpose-named repositories** (e.g. a submission-status store), not EF Core and
not a generic `IRepository<T>`. The store sits behind the `ISubmissionStatusStore` port so the demo
can use an in-memory implementation (see [ADR-0004](ADR-0004-in-memory-adapters-for-demo.md)).

## Consequences

- **Positive**: explicit SQL, fast cold paths, no change-tracker/migration surprises, easy to reason
  about in review.
- **Negative**: hand-written SQL and mapping; no automatic migrations (schema is managed separately).
- **Scope note**: this demo ships only the in-memory store; the Dapper implementation is the
  documented production swap and not included here.
