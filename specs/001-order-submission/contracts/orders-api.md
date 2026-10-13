# Orders API Contract

## POST /api/orders

Submit a purchase order for asynchronous processing.

**Request body**

```json
{
  "legalEntityId": "ABN-01",
  "supplier": "ACME Pty Ltd",
  "clientRequestId": "req-10001",
  "lines": [
    { "sku": "TIMBER-90X45", "quantity": 120, "unitPrice": 7.85 },
    { "sku": "NAILS-75MM",   "quantity": 50,  "unitPrice": 19.90 }
  ]
}
```

**Responses**

| Status | Body | When |
|--------|------|------|
| `202 Accepted` | `{ "correlationId": "…" }` | Order valid and accepted (or idempotent replay) |
| `400 Bad Request` | `{ "errors": { "field": ["message"] } }` | Validation failed; nothing published |

## GET /api/orders/{correlationId}/status

Read the latest known status.

| Status | Body | When |
|--------|------|------|
| `200 OK` | `{ "correlationId": "…", "state": "Accepted", "lastUpdated": "…" }` | Known correlation id |
| `404 Not Found` | — | Unknown correlation id |

`state` ∈ `Accepted | Submitted | Completed | Failed`.
