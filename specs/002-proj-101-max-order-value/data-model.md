# Data Model: Reject Purchase Orders Exceeding a Maximum Total Value

## MaxOrderValueOptions (new)

Configuration-bound options record — no persistence, resolved once at startup per environment.

| Field | Type | Default | Notes |
|-------|------|---------|-------|
| `Threshold` | `decimal` | `250_000.00` (AUD) | Must be `> 0`; validated at startup (`ValidateOnStart`), fails fast if misconfigured (FR-007). |

Bound from configuration section `MaxOrderValue` (e.g. `MaxOrderValue:Threshold` in
`local.settings.json` / App Configuration), so an environment can override it without a code
change (FR-004).

## PurchaseOrder (existing — derived value only)

No new persisted fields. This feature adds a **derived, in-memory-only** calculation:

| Derived value | Formula | Used by |
|---------------|---------|---------|
| `orderTotal` | `Σ (line.Quantity × line.UnitPrice)` across `order.Lines` | `PurchaseOrderValidator` rule naming `orderTotal`; rejection log message |

`orderTotal` is never stored on `PurchaseOrder`, `SubmitEnvelope`, or `SubmissionStatus` — it is
computed on demand for validation and logging only.

## Validation Rule

| Rule | Condition | Error field | Response |
|------|-----------|-------------|----------|
| Max order value | `orderTotal > MaxOrderValueOptions.Threshold` | `orderTotal` | `400 Bad Request`, nothing published |

Runs alongside existing [001-order-submission](../001-order-submission/spec.md#key-entities)
field-level rules in the same `PurchaseOrderValidator`; evaluated before
`SubmissionService.SubmitAsync` is ever called.
