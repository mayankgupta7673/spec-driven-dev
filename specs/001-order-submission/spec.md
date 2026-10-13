# Feature Specification: Asynchronous Purchase Order Submission

**Feature Branch**: `001-order-submission`

**Created**: 2026-10-13

**Status**: Draft

**Input**: User description: "Submit a purchase order to D365 asynchronously via Service Bus and return 202 with a correlation id"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Submit a purchase order and receive acknowledgement (Priority: P1)

A caller (upstream system) submits a purchase order through the public API. The system accepts
the order, hands it off for asynchronous processing to D365, and immediately returns a
`202 Accepted` with a correlation id the caller can use to track status.

**Why this priority**: This is the core value — decoupling the caller from slow D365 writes.
Without it there is no feature. It is the MVP on its own.

**Independent Test**: POST a valid order to the endpoint and assert a `202` response body
containing a non-empty `correlationId`, and assert a message was published to the submit topic.

**Acceptance Scenarios**:

1. **Given** a well-formed purchase order, **When** it is POSTed to the submit endpoint, **Then** the system returns `202 Accepted` with a `correlationId`.
2. **Given** a well-formed purchase order, **When** it is accepted, **Then** exactly one typed envelope is published to the submit topic with the session id set to the order's legal-entity id.

---

### User Story 2 - Reject invalid orders early (Priority: P2)

A caller submits a malformed or incomplete order. The system validates at the boundary and
returns `400 Bad Request` with a machine-readable list of validation errors, without publishing
anything to the bus.

**Why this priority**: Prevents poison messages and gives callers fast, actionable feedback.

**Independent Test**: POST an order missing a required field and assert `400` with the offending
field named, and assert no message was published.

**Acceptance Scenarios**:

1. **Given** an order with no line items, **When** it is POSTed, **Then** the system returns `400` naming `lines` as required and publishes nothing.
2. **Given** an order with an unknown legal-entity id, **When** it is POSTed, **Then** the system returns `400` naming `legalEntityId`.

---

### User Story 3 - Query submission status by correlation id (Priority: P3)

A caller uses the previously returned correlation id to read the current status of the
submission (Accepted, Submitted, Completed, Failed).

**Why this priority**: Observability for callers; valuable but not required for the first slice.

**Independent Test**: Given a known correlation id with a recorded status, GET the status
endpoint and assert the returned status matches.

**Acceptance Scenarios**:

1. **Given** a correlation id that was accepted, **When** its status is queried, **Then** the system returns the latest known status.
2. **Given** an unknown correlation id, **When** its status is queried, **Then** the system returns `404`.

### Edge Cases

- What happens when the same order is submitted twice (idempotency on a client-supplied request id)?
- How does the system handle a Service Bus publish failure after validation has passed?
- What happens when the legal-entity id is valid in format but not mapped to a D365 company?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose an HTTP endpoint that accepts a purchase order payload and returns `202 Accepted` with a correlation id on success.
- **FR-002**: System MUST validate the inbound order at the boundary and return `400` with per-field errors when invalid, publishing nothing.
- **FR-003**: System MUST publish exactly one typed envelope to the submit topic per accepted order, with the Service Bus session id set to the order's legal-entity id.
- **FR-004**: System MUST record an initial status of `Accepted` keyed by correlation id at acceptance time.
- **FR-005**: System MUST log every acceptance and rejection with the `{CorrelationId}` in the log scope.
- **FR-006**: System MUST treat a repeated client request id as idempotent and return the original correlation id without publishing a second message.
- **FR-007**: System MUST expose a status endpoint that returns the latest known status for a correlation id, or `404` if unknown.

### Key Entities *(include if feature involves data)*

- **PurchaseOrder**: The inbound contract — header fields (legal-entity id, supplier, client request id) and one or more order lines.
- **SubmitEnvelope**: The typed message published to the bus — wraps the order plus correlation id and session id.
- **SubmissionStatus**: Correlation id, current state (Accepted/Submitted/Completed/Failed), and last-updated timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A valid order receives a `202` with a correlation id in under 300 ms at the API boundary (excludes downstream D365 time).
- **SC-002**: 100% of accepted orders result in exactly one bus message (no duplicates, no drops) under normal operation.
- **SC-003**: 100% of malformed orders are rejected at the boundary with a named offending field and zero bus messages.
- **SC-004**: Every accepted and rejected request is traceable end-to-end by its correlation id in logs.

## Assumptions

- The submit topic and its subscription already exist and are reachable via Managed Identity.
- A legal-entity → D365 company mapping is available to the validator.
- The downstream D365 submit pipeline (poller, DMF import) is out of scope for this feature and handled by shared infrastructure.
- Status is read-only here; the writer of `Submitted/Completed/Failed` transitions is the shared poller, out of scope for this slice.
