# Orders API Contract — Addendum (PROJ-101: Max Order Value)

Extends [../001-order-submission/contracts/orders-api.md](../../001-order-submission/contracts/orders-api.md).
No endpoint shape changes — only a new rejection case on the existing `POST /api/orders`.

## POST /api/orders — new rejection case

**New `400` case**

| Status | Body | When |
|--------|------|------|
| `400 Bad Request` | `{ "errors": { "orderTotal": ["orderTotal 275000.00 exceeds the maximum allowed value of 250000.00."] } }` | `Σ quantity × unitPrice` across all lines exceeds the configured `MaxOrderValue:Threshold`; nothing published |

Example rejected request (total = `120 × 7.85 + 50 × 19.90 = 1,942.00`, assume threshold lowered
to `1000` for illustration):

```json
{
  "legalEntityId": "ABN-01",
  "supplier": "ACME Pty Ltd",
  "clientRequestId": "req-10002",
  "lines": [
    { "sku": "TIMBER-90X45", "quantity": 120, "unitPrice": 7.85 },
    { "sku": "NAILS-75MM",   "quantity": 50,  "unitPrice": 19.90 }
  ]
}
```

Response:

```json
{ "errors": { "orderTotal": ["orderTotal 1942.00 exceeds the maximum allowed value of 1000.00."] } }
```

## Configuration

| Setting | Default | Notes |
|---------|---------|-------|
| `MaxOrderValue:Threshold` | `250000` (AUD) | Bound via `IOptions<MaxOrderValueOptions>`; validated at startup; override per environment in `local.settings.json` / App Configuration. |
