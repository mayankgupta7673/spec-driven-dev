# ADR-0004 — In-memory adapters behind interfaces for the offline demo

**Status**: Accepted · **Date**: 2026-10-13

## Context

The demo must run on a laptop with no Azure resources, yet remain faithful to the production
architecture so the client sees the real shape of the code.

## Decision

Define ports for the external dependencies and provide **in-memory implementations** for the demo:

| Port | Demo adapter | Production adapter |
|------|--------------|--------------------|
| `IMessagePublisher` | `InMemoryMessagePublisher` | Azure Service Bus publisher |
| `ISubmissionStatusStore` | `InMemorySubmissionStatusStore` | Azure SQL (Dapper) store |
| `ILegalEntityMap` | `StaticLegalEntityMap` | config/SQL-backed map |

Selection happens in `Program.cs`. Switching to Azure is a **DI change only** — Functions, services,
models, and tests are untouched.

## Consequences

- **Positive**: zero-dependency demo, deterministic tests, a clean seam for production wiring,
  a compelling "two-line swap" talking point.
- **Negative**: in-memory adapters do not exercise real transport/storage semantics (ordering,
  retries, transactions); those are validated separately with integration tests in production.
- `AzureWebJobsStorage` is intentionally empty for the HTTP-only offline run and must be set before
  deployment.
