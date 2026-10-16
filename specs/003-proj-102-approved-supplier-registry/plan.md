# Implementation Plan: Validate Supplier Against an Approved Supplier Registry

**Branch**: `003-proj-102-approved-supplier-registry` | **Date**: 2026-10-15 | **Spec**: [spec.md](spec.md)

## Summary

Add a boundary validation rule to the existing `/api/orders` submit pipeline (from
[001-order-submission](../001-order-submission/plan.md)) that rejects any order whose `supplier`
is not present in an approved supplier registry, returning `400 Bad Request` naming `supplier`
and publishing nothing. The registry sits behind a new port, `ISupplierRegistry`, with an
in-memory demo implementation (`StaticSupplierRegistry`) — the same shape as the existing
`ILegalEntityMap` / `StaticLegalEntityMap` pair — swappable for a config/SQL-backed
implementation in production without touching the validator or Function.

This is a pure extension of the existing vertical slice (same `SddDemo.Functions` context, same
endpoint) — no new Function, no new Azure dependency, fully offline/in-memory as today.

## Technical Context

- **.NET 8 isolated worker**, C# 12 — same project as [001-order-submission](../001-order-submission/plan.md).
- **FluentValidation** — the new rule is added to the existing `PurchaseOrderValidator`, reusing
  the existing validate → `400` → "nothing published" pipeline in `SubmitOrderFunction`.
- No `IOptions<T>` needed — the approved list is a fixed in-memory demo set (per spec Assumptions),
  mirroring `StaticLegalEntityMap` rather than `MaxOrderValueOptions`.
- No new Azure SDK surface; no change to `IMessagePublisher` / `ISubmissionStatusStore`.

## Architecture (thin adapter, rich core)

```
HTTP POST /api/orders ──► SubmitOrderFunction (unchanged: bind, validate, dispatch, return)
                                   │
                                   ▼
                     IValidator<PurchaseOrder> = PurchaseOrderValidator
                                   │
                                   ├─ existing rules (001-order-submission, 002-proj-101)
                                   └─ NEW: ISupplierRegistry.IsApproved(Supplier)
                                   │
                                   ▼ (only if valid)
                          ISubmissionService.SubmitAsync (unchanged)
```

The rule lives entirely in `Validation/PurchaseOrderValidator.cs` (no Azure SDK, no I/O) — it
runs and is rejected before `SubmissionService`/`IMessagePublisher` are ever invoked, so "nothing
is published" (FR-001) is a natural consequence of the existing control flow, not new logic to
re-implement.

## Project Structure (additions to 001-order-submission / 002-proj-101-max-order-value)

```
src/SddDemo.Functions/
├── Services/
│   ├── ISupplierRegistry.cs        # NEW: port — IsApproved(string supplier)
│   └── StaticSupplierRegistry.cs   # NEW: in-memory demo adapter (case-insensitive set)
├── Validation/
│   └── PurchaseOrderValidator.cs   # MODIFIED: + supplier-approved rule, takes ISupplierRegistry
└── Program.cs                      # MODIFIED: register ISupplierRegistry -> StaticSupplierRegistry
```

## Constitution Check

- ✅ Thin adapters: `SubmitOrderFunction` is untouched; new logic lives in `Services/` and `Validation/`.
- ✅ No Azure SDK in `Services/`/`Validation/`; `*.Functions.Tests` still runs offline.
- ✅ Purpose-named port (`ISupplierRegistry`), not a generic `IRepository<T>`.
- ✅ No `static` mutable state (the demo set is a `private static readonly` immutable `HashSet`,
  same pattern already accepted in `StaticLegalEntityMap`); no new `HttpClient`; no
  Helper/Manager/Util class.
- ✅ Structured logging for the rejection path (FR-006).
- ✅ `sealed` by default for the new adapter class.

## Phase 0 — Research

No open unknowns. The feature is a direct structural clone of the existing
`ILegalEntityMap` / `StaticLegalEntityMap` port-and-adapter pair, just keyed on `Supplier`
instead of `LegalEntityId` — no new pattern to evaluate.

## Phase 1 — Design & Contracts

- See [data-model.md](data-model.md) for the new `ISupplierRegistry` port and
  `StaticSupplierRegistry` adapter.
- Contract update: [contracts/orders-api.md](contracts/orders-api.md) documents the new `400`
  rejection case naming `supplier`.
- No new quickstart needed — exercised via the existing `/api/orders` endpoint; see
  [demo.http](../../demo.http) for manual verification requests.

## Complexity Tracking

None. Single rule added to an existing validator in the same bounded context, using the same
port-and-adapter shape already present in the codebase; no new deployable, no new cross-cutting
concern.
