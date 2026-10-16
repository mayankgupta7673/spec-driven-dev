# Data Model: Validate Supplier Against an Approved Supplier Registry

## ISupplierRegistry (new port)

Resolves whether a supplier name is approved. No persistence concerns at the port level.

```csharp
public interface ISupplierRegistry
{
    bool IsApproved(string supplier);
}
```

| Member | Notes |
|--------|-------|
| `IsApproved(string supplier)` | Case-insensitive, `Ordinal`-based comparison; empty/whitespace input returns `false`. |

## StaticSupplierRegistry (new demo adapter)

In-memory implementation for this ticket — same shape as the existing `StaticLegalEntityMap`.
Swappable for a config/SQL-backed implementation in production (FR-004) without changing
`PurchaseOrderValidator` or `SubmitOrderFunction`.

| Field | Type | Notes |
|-------|------|-------|
| `Known` | `HashSet<string>` (`OrdinalIgnoreCase`) | Fixed, hard-coded demo set of approved supplier names. |

## PurchaseOrder (existing — no new fields)

This feature adds a new validation rule against the existing `Supplier` field — no new field is
added to `PurchaseOrder`, `SubmitEnvelope`, or `SubmissionStatus`.

## Validation Rule

| Rule | Condition | Error field | Response |
|------|-----------|-------------|----------|
| Approved supplier | `!supplierRegistry.IsApproved(order.Supplier)` | `supplier` | `400 Bad Request`, nothing published |

Runs alongside existing [001-order-submission](../001-order-submission/spec.md#key-entities)
field-level rules and the [002-proj-101-max-order-value](../002-proj-101-max-order-value/data-model.md)
order-total rule in the same `PurchaseOrderValidator`; evaluated before
`SubmissionService.SubmitAsync` is ever called.
