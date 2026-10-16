# Orders API Contract — Addendum (PROJ-102: Approved Supplier Registry)

Extends [../001-order-submission/contracts/orders-api.md](../../001-order-submission/contracts/orders-api.md)
and [../002-proj-101-max-order-value/contracts/orders-api.md](../../002-proj-101-max-order-value/contracts/orders-api.md).
No endpoint shape changes — only a new rejection case on the existing `POST /api/orders`.

## POST /api/orders — new rejection case

**New `400` case**

| Status | Body | When |
|--------|------|------|
| `400 Bad Request` | `{ "errors": { "Supplier": ["supplier is not an approved supplier."] } }` | `supplier` is not present in the approved supplier registry (case-insensitive); nothing published |

Example rejected request:

```json
{
  "legalEntityId": "ABN-01",
  "supplier": "Unknown Supplier Pty Ltd",
  "clientRequestId": "req-10003",
  "lines": [
    { "sku": "TIMBER-90X45", "quantity": 10, "unitPrice": 7.85 }
  ]
}
```

Response:

```json
{ "errors": { "Supplier": ["supplier is not an approved supplier."] } }
```

## Configuration

None. The approved-supplier list is a fixed in-memory demo set (`StaticSupplierRegistry`), not
configuration-bound. A production swap (config/SQL-backed `ISupplierRegistry`) is handled by the
`azure` agent and does not change this contract.
