# ADR-0002 — Function triggers are thin adapters

**Status**: Accepted · **Date**: 2026-10-13

## Context

Business logic embedded in Azure Function trigger classes is hard to unit test (requires the host),
couples domain rules to the Azure SDK, and resists reuse.

## Decision

Function triggers are **thin adapters** (~30 lines): bind the request, validate, dispatch to a
service, and return a result. All real logic lives in `Services/`, `Models/`, and `Validation/`,
which carry **zero Azure SDK references**.

Example: `SubmitOrderFunction` deserializes the body, runs `PurchaseOrderValidator`, calls
`ISubmissionService.SubmitAsync`, and maps the result to `202` / `400`. It contains no submission,
idempotency, or persistence logic.

## Consequences

- **Positive**: the core is unit-testable offline; the same services work behind any trigger
  (HTTP, queue, timer); swapping transports does not touch logic.
- **Negative**: more types/indirection than putting code in the trigger.
- **Enforcement**: `tests/SddDemo.Functions.Tests` must run with no Azure packages — the litmus test.
