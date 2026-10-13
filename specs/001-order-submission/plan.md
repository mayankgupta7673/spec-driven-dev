# Implementation Plan: Asynchronous Purchase Order Submission

**Branch**: `001-order-submission` | **Date**: 2026-10-13 | **Spec**: [spec.md](spec.md)

## Summary

Expose an HTTP API that accepts a purchase order, validates it at the boundary, records an
initial `Accepted` status keyed by correlation id, and publishes one typed envelope to the
submit topic (session = legal-entity id). A second HTTP endpoint reads status by correlation id.
Idempotency is keyed on a client-supplied request id.

For the demo, the bus and status store sit behind interfaces with **in-memory implementations**
so the app runs fully offline. Swapping to Azure Service Bus + Azure SQL is a DI change only —
no change to Functions, services, or models.

## Technical Context

- **.NET 8 isolated worker**, C# 12, HTTP triggers via ASP.NET Core integration.
- **FluentValidation** for the inbound contract.
- **System.Text.Json** for (de)serialization.
- `TimeProvider` injected for timestamps; `Guid` for correlation ids.
- Logging via `ILogger<T>` with `{CorrelationId}` in scope.

## Architecture (thin adapter, rich core)

```
HTTP POST /api/orders ──► SubmitOrderFunction (adapter: bind, validate, dispatch, return)
                                   │
                                   ▼
                          ISubmissionService ──► IMessagePublisher   (InMemory | ServiceBus)
                                   │           └► ISubmissionStatusStore (InMemory | AzureSql)
                                   └► ILegalEntityMap (validation support)

HTTP GET /api/orders/{correlationId}/status ──► GetStatusFunction ──► ISubmissionStatusStore
```

## Project Structure

```
src/SddDemo.Functions/
├── Functions/
│   ├── SubmitOrderFunction.cs      # HTTP POST adapter
│   └── GetStatusFunction.cs        # HTTP GET adapter
├── Models/
│   ├── PurchaseOrder.cs
│   ├── OrderLine.cs
│   ├── SubmitEnvelope.cs
│   ├── SubmissionState.cs
│   ├── SubmissionStatus.cs
│   └── SubmissionResult.cs
├── Validation/
│   └── PurchaseOrderValidator.cs
├── Services/
│   ├── ISubmissionService.cs / SubmissionService.cs
│   ├── IMessagePublisher.cs / InMemoryMessagePublisher.cs
│   ├── ISubmissionStatusStore.cs / InMemorySubmissionStatusStore.cs
│   └── ILegalEntityMap.cs / StaticLegalEntityMap.cs
└── Program.cs                      # DI composition root
```

## Constitution Check

- ✅ Thin adapters: triggers only bind/validate/dispatch/return.
- ✅ Inner folders carry no Azure SDK references; tests run offline.
- ✅ `records`, `sealed`, `CancellationToken`, structured logging, `TimeProvider`.
- ✅ No `static` mutable state, no `new HttpClient()`, no generic repository.
- ✅ Idempotency + correlation id traceability (FR-006, FR-005).

## Phase 0 — Research

No open unknowns. In-memory adapters chosen for demo determinism; production swap documented above.

## Phase 1 — Contracts & Data

- Request: `PurchaseOrder { legalEntityId, supplier, clientRequestId, lines[] { sku, quantity, unitPrice } }`.
- Accept response: `202 { correlationId }`.
- Status response: `200 { correlationId, state, lastUpdated }` or `404`.
- See [contracts/orders-api.md](contracts/orders-api.md).

## Complexity Tracking

None. Single bounded context, single deployable, no cross-cutting changes.
