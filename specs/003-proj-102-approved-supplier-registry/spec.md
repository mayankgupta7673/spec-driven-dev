# Feature Specification: Validate Supplier Against an Approved Supplier Registry

**Feature Branch**: `003-proj-102-approved-supplier-registry`

**Created**: 2026-10-15

**Status**: Draft

**Input**: JIRA issue [PROJ-102](https://jira.example.com/browse/PROJ-102) — "Validate supplier against an approved supplier registry" (Story, Parent: PROJ-100, Priority: Medium, 3 points)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Known supplier is accepted (Priority: P1)

A caller submits a purchase order whose `supplier` is present in the approved supplier registry.
The system accepts and processes the order exactly as it does today — no new friction for
legitimate suppliers.

**Why this priority**: This is the regression guard — the new validation must not break the
existing happy path (see [001-order-submission](../001-order-submission/spec.md) and
[002-proj-101-max-order-value](../002-proj-101-max-order-value/spec.md)). It is the baseline the
rejection rule is layered on top of.

**Independent Test**: POST an order whose `supplier` exists in the approved registry and assert
`202 Accepted` with a `correlationId`, and assert exactly one envelope is published.

**Acceptance Scenarios**:

1. **Given** a supplier that exists in the approved registry, **When** the order is POSTed to `/api/orders`, **Then** the response is `202 Accepted`.
2. **Given** a supplier that exists in the approved registry, **When** the order is POSTed, **Then** exactly one envelope is published to the submit transport.

---

### User Story 2 - Unknown supplier is rejected (Priority: P1)

A caller submits a purchase order whose `supplier` is not present in the approved supplier
registry. The system must reject it at the boundary — before publishing to the submit transport —
so D365 is never exposed to orders from unapproved suppliers.

**Why this priority**: This is the core value of the ticket. Preventing orders from unapproved
suppliers reaching D365 is the entire reason the feature exists; without this rule nothing is
delivered.

**Independent Test**: POST an order whose `supplier` is not in the approved registry and assert
`400 Bad Request` naming `supplier`, and assert nothing was published to the bus.

**Acceptance Scenarios**:

1. **Given** a supplier that is not in the approved registry, **When** the order is POSTed to `/api/orders`, **Then** the response is `400 Bad Request` naming `supplier`.
2. **Given** a supplier that is not in the approved registry, **When** the order is POSTed, **Then** nothing is published to the submit transport.

---

### User Story 3 - Registry is swappable without changing callers (Priority: P3)

An operator swaps the approved-supplier source (demo in-memory list today; config- or SQL-backed
in production) without changing `SubmitOrderFunction`, the validator's calling contract, or any
caller of `/api/orders`.

**Why this priority**: Keeps the registry's backing store a pure implementation detail behind a
port, consistent with the project's hexagonal architecture, but is not required for the first
working version of the rule (which can ship with the documented in-memory demo list).

**Independent Test**: Replace the `ISupplierRegistry` implementation registered in DI with a
different in-memory list and assert the same acceptance/rejection behavior, with no change to
`PurchaseOrderValidator`, `SubmitOrderFunction`, or the API contract.

**Acceptance Scenarios**:

1. **Given** a different `ISupplierRegistry` implementation is registered, **When** an order is submitted for a supplier approved under the new implementation, **Then** it is accepted without any code change to the validator or Function.

---

### Edge Cases

- What happens when `supplier` is empty or whitespace? Already rejected by existing
  [001-order-submission](../001-order-submission/spec.md) `FR` requiring `supplier` to be
  non-empty; this ticket only adds the registry-membership check and only runs it when `supplier`
  is non-empty.
- What happens when the registry lookup is case-sensitive vs. case-insensitive? Supplier name
  matching is case-insensitive, consistent with the existing `legalEntityId` registry pattern
  (`ILegalEntityMap`/`StaticLegalEntityMap`).
- What happens when the approved registry itself is empty (misconfigured)? Out of scope for this
  ticket — the demo in-memory registry ships with a non-empty default list; a production
  config/SQL-backed registry's own emptiness/availability concerns are deferred to the `azure`
  agent's swap-in work.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST reject (return `400 Bad Request` naming `supplier`) any order whose
  `supplier` is not present in the approved supplier registry, and MUST NOT publish anything to
  the submit transport for that order.
- **FR-002**: System MUST accept (continue existing `202 Accepted` behavior) any order whose
  `supplier` is present in the approved supplier registry.
- **FR-003**: The approved-supplier lookup MUST be performed case-insensitively, consistent with
  the existing `legalEntityId` lookup pattern.
- **FR-004**: The approved-supplier source MUST sit behind a port (`ISupplierRegistry`) with an
  in-memory demo implementation for this ticket, swappable for a config/SQL-backed implementation
  in production without changing the validator or Function.
- **FR-005**: System MUST apply this check in addition to, not instead of, existing boundary
  validation (field-level, legal-entity, and order-total checks) defined in
  [001-order-submission](../001-order-submission/spec.md) and
  [002-proj-101-max-order-value](../002-proj-101-max-order-value/spec.md).
- **FR-006**: System MUST log the rejection (including the rejected `supplier` value) when an
  order is rejected for an unapproved supplier, consistent with existing boundary-rejection
  logging.

### Key Entities *(include if feature involves data)*

- **PurchaseOrder**: Existing entity from [001-order-submission](../001-order-submission/spec.md);
  this feature adds a new validation rule against the existing `Supplier` field — no new field.
- **ISupplierRegistry**: New port — resolves whether a supplier name is approved. Demo
  implementation (`StaticSupplierRegistry`) holds an in-memory approved list; production
  implementations (config/SQL-backed) are out of scope for this ticket.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of orders whose `supplier` is not in the approved registry are rejected with
  `400` naming `supplier`, with zero bus messages published.
- **SC-002**: 100% of orders whose `supplier` is in the approved registry continue to be accepted
  with `202` and exactly one published envelope (no regression to
  [001-order-submission](../001-order-submission/spec.md) or
  [002-proj-101-max-order-value](../002-proj-101-max-order-value/spec.md)).
- **SC-003**: Swapping the registry's backing implementation requires zero changes to
  `PurchaseOrderValidator`, `SubmitOrderFunction`, or the `/api/orders` contract.

## Assumptions

- The approved supplier registry is a single global list per environment (not per
  legal-entity) — the ticket does not describe per-dimension supplier approval.
- This validation runs within the same `SddDemo.Functions` context/vertical slice as
  [001-order-submission](../001-order-submission/spec.md) (same `/api/orders` endpoint), not a
  new Function App.
- The demo in-memory approved-supplier list is a fixed, hard-coded set for this ticket (mirroring
  `StaticLegalEntityMap`), not configuration-bound — the ticket does not ask for per-environment
  configurability of the list itself (unlike PROJ-101's threshold).
- [Dependency on existing system/service, e.g., "Requires access to the existing user profile API"]
