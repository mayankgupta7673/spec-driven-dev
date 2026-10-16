# Implementation Plan: Reject Purchase Orders Exceeding a Maximum Total Value

**Branch**: `002-proj-101-max-order-value` | **Date**: 2026-10-15 | **Spec**: [spec.md](spec.md)

## Summary

Add a boundary validation rule to the existing `/api/orders` submit pipeline (from
[001-order-submission](../001-order-submission/plan.md)) that rejects any order whose total
value (`Σ quantity × unitPrice`) exceeds a configurable threshold, returning `400 Bad Request`
naming `orderTotal` and publishing nothing. The threshold is bound from configuration via
`IOptions<MaxOrderValueOptions>` (default **AUD 250,000**), so changing it per environment is a
configuration-only change — no code or redeploy of business logic.

This is a pure extension of the existing vertical slice (same `SddDemo.Functions` context, same
endpoint) — no new Function, no new Azure dependency, fully offline/in-memory as today.

## Technical Context

- **.NET 8 isolated worker**, C# 12 — same project as [001-order-submission](../001-order-submission/plan.md).
- **FluentValidation** — the new rule is added to the existing `PurchaseOrderValidator`, reusing
  the existing validate → `400` → "nothing published" pipeline in `SubmitOrderFunction`.
- **`Microsoft.Extensions.Options`** — `MaxOrderValueOptions` bound from configuration
  (`MaxOrderValue:Threshold`), validated at startup (`ValidateOnStart`) per FR-007.
- No new Azure SDK surface; no change to `IMessagePublisher` / `ISubmissionStatusStore`.

## Architecture (thin adapter, rich core)

```
HTTP POST /api/orders ──► SubmitOrderFunction (unchanged: bind, validate, dispatch, return)
                                   │
                                   ▼
                     IValidator<PurchaseOrder> = PurchaseOrderValidator
                                   │
                                   ├─ existing rules (001-order-submission)
                                   └─ NEW: orderTotal <= IOptions<MaxOrderValueOptions>.Threshold
                                   │
                                   ▼ (only if valid)
                          ISubmissionService.SubmitAsync (unchanged)
```

The rule lives entirely in `Validation/PurchaseOrderValidator.cs` (no Azure SDK, no I/O) — it
runs and is rejected before `SubmissionService`/`IMessagePublisher` are ever invoked, so "nothing
is published" (FR-002) is a natural consequence of the existing control flow, not new logic to
re-implement.

## Project Structure (additions to 001-order-submission)

```
src/SddDemo.Functions/
├── Models/
│   ├── MaxOrderValueOptions.cs     # NEW: { Threshold } options record, default 250_000m
│   └── PurchaseOrderExtensions.cs  # NEW: GetOrderTotal() — Σ quantity × unitPrice
├── Validation/
│   └── PurchaseOrderValidator.cs   # MODIFIED: + orderTotal rule, takes IOptions<MaxOrderValueOptions>
└── Program.cs                      # MODIFIED: bind + validate MaxOrderValueOptions at startup
```

## Constitution Check

- ✅ Thin adapters: `SubmitOrderFunction` is untouched; new logic lives in `Validation/`.
- ✅ No Azure SDK in `Models/`/`Validation/`; `*.Functions.Tests` still runs offline.
- ✅ `record` for the new options type; `sealed` by default.
- ✅ Configuration via `IOptions<T>`, not hard-coded — satisfies FR-004.
- ✅ No `static` mutable state; no new `HttpClient`; no generic repository; no Helper/Manager/Util class.
- ✅ Structured logging for the rejection path (FR-006), no correlation id implied (none exists pre-acceptance).

## Phase 0 — Research

No open unknowns. `IOptions<T>` + `ValidateOnStart()` is the standard .NET pattern for fail-fast
config validation (FR-007); no alternatives considered since this matches existing project
conventions (`TimeProvider` is also DI-bound, not static).

## Phase 1 — Design & Contracts

- See [data-model.md](data-model.md) for the new `MaxOrderValueOptions` entity and the derived
  `orderTotal` calculation.
- Contract update: [contracts/orders-api.md](contracts/orders-api.md) documents the new `400`
  rejection case naming `orderTotal`.
- No new quickstart needed — exercised via the existing `/api/orders` endpoint; see
  [demo.http](../../demo.http) for manual verification requests.

## Complexity Tracking

None. Single rule added to an existing validator in the same bounded context; no new deployable,
no new cross-cutting concern.
