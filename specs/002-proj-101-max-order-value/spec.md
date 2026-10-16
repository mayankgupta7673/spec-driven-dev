# Feature Specification: Reject Purchase Orders Exceeding a Maximum Total Value

**Feature Branch**: `002-proj-101-max-order-value`

**Created**: 2026-10-15

**Status**: Draft

**Input**: JIRA issue [PROJ-101](https://jira.example.com/browse/PROJ-101) — "Reject purchase orders that exceed a maximum total value" (Story, Parent: PROJ-100, Priority: High, 3 points)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Order within the limit is accepted (Priority: P1)

A caller submits a purchase order whose total value (sum of `quantity × unitPrice` across all
lines) is less than or equal to the configured threshold. The system accepts and processes the
order exactly as it does today — no new friction for legitimate orders.

**Why this priority**: This is the regression guard — the new validation must not break the
existing happy path (see [001-order-submission](../001-order-submission/spec.md)). It is the
baseline the rejection rule is layered on top of.

**Independent Test**: POST an order whose computed total is at or below the configured threshold
and assert `202 Accepted` with a `correlationId`, and assert exactly one envelope is published.

**Acceptance Scenarios**:

1. **Given** an order whose total is less than or equal to the configured threshold, **When** it is POSTed to `/api/orders`, **Then** the response is `202 Accepted` with a `correlationId`.
2. **Given** an order whose total equals the configured threshold exactly, **When** it is POSTed, **Then** the response is `202 Accepted` and exactly one envelope is published.

---

### User Story 2 - Order above the limit is rejected (Priority: P1)

A caller submits a purchase order whose total value exceeds the configured threshold. The system
must reject it at the boundary — before publishing to the submit transport — so D365 is never
exposed to erroneous bulk orders.

**Why this priority**: This is the core value of the ticket. Protecting D365 from erroneous bulk
orders is the entire reason the feature exists; without this rule nothing is delivered.

**Independent Test**: POST an order whose computed total exceeds the configured threshold and
assert `400 Bad Request` naming `orderTotal`, and assert nothing was published to the bus.

**Acceptance Scenarios**:

1. **Given** an order whose total exceeds the configured threshold, **When** it is POSTed to `/api/orders`, **Then** the response is `400 Bad Request` naming `orderTotal`.
2. **Given** an order whose total exceeds the configured threshold, **When** it is POSTed, **Then** nothing is published to the submit transport.

---

### User Story 3 - Threshold is configurable per environment (Priority: P2)

An operator changes the maximum-order-value threshold for an environment via configuration
(options), without any code change or redeploy of business logic.

**Why this priority**: Keeps the guardrail tunable across environments (e.g. lower in test,
higher in production) without a release cycle, but is not required for the first working version
of the rule (which can ship with the documented default).

**Independent Test**: Change the configured threshold value via options/configuration only, and
assert the validator honours the new value on the next request without a code change.

**Acceptance Scenarios**:

1. **Given** the threshold is supplied via options/configuration, **When** an environment changes the value, **Then** no code change is required for the new threshold to take effect.
2. **Given** no threshold is explicitly configured, **When** the system starts, **Then** it falls back to the documented default of **AUD 250,000**.

---

### Edge Cases

- What happens when the order total is exactly equal to the threshold? (Treated as within limit — see User Story 1, Scenario 2.)
- What happens when an order has zero lines (no total to compute)? Existing `FR-002` from [001-order-submission](../001-order-submission/spec.md) already rejects orders with no lines before this rule runs.
- What happens when a line's `quantity` or `unitPrice` is negative or zero? Out of scope for this ticket — independent field-level validity of `quantity`/`unitPrice` is governed by existing [001-order-submission](../001-order-submission/spec.md) validation, not this rule.
- What happens when the configured threshold itself is misconfigured (e.g. negative or zero)? The threshold is validated once at startup (options validation) and the application fails fast with a configuration error; this is not a per-request concern.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST compute an order's total value as the sum of `quantity × unitPrice` across all order lines before publishing to the submit transport.
- **FR-002**: System MUST reject (return `400 Bad Request` naming `orderTotal`) any order whose total value exceeds the configured maximum-order-value threshold, and MUST NOT publish anything to the submit transport for that order.
- **FR-003**: System MUST accept (continue existing `202 Accepted` behavior) any order whose total value is less than or equal to the configured threshold.
- **FR-004**: The maximum-order-value threshold MUST be read from configuration/options (not hard-coded), MUST support a different value per environment, and MUST default to **AUD 250,000** when not explicitly set.
- **FR-005**: System MUST apply this check in addition to, not instead of, existing boundary validation (field-level and legal-entity checks) defined in [001-order-submission](../001-order-submission/spec.md).
- **FR-006**: System MUST log the rejection (including the rejected `legalEntityId` and the computed `orderTotal`) when an order is rejected for exceeding the threshold, consistent with existing boundary-rejection logging — no correlation id exists yet for a rejected order.
- **FR-007**: System MUST validate the configured threshold is a positive value at startup and fail fast with a configuration error if it is not.

### Key Entities *(include if feature involves data)*

- **PurchaseOrder**: Existing entity from [001-order-submission](../001-order-submission/spec.md); this feature adds a derived `orderTotal` (sum of `quantity × unitPrice` across lines) used only for validation, not persisted as a new field.
- **MaxOrderValueOptions**: New configuration entity — a single configurable threshold value (currency: AUD, default 250,000) bound via options, resolved per environment.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of orders whose total exceeds the configured threshold are rejected with `400` naming `orderTotal`, with zero bus messages published.
- **SC-002**: 100% of orders at or below the configured threshold continue to be accepted with `202` and exactly one published envelope (no regression to [001-order-submission](../001-order-submission/spec.md)).
- **SC-003**: Changing the threshold for an environment requires a configuration change only — zero code changes or redeploys of business logic.

## Assumptions

- The threshold is a single global value per environment (not per legal-entity, supplier, or customer) — the ticket does not describe per-dimension thresholds.
- The default of AUD 250,000 applies when no override is configured, in every environment including production.
- Order lines' `quantity` and `unitPrice` are already validated as present and numeric by existing [001-order-submission](../001-order-submission/spec.md) field-level validation; this feature only adds the aggregate-total check.
- This validation runs within the same `SddDemo.Functions` context/vertical slice as [001-order-submission](../001-order-submission/spec.md) (same `/api/orders` endpoint), not a new Function App.
