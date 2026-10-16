# Tasks: Validate Supplier Against an Approved Supplier Registry

**Input**: [spec.md](spec.md), [plan.md](plan.md), [data-model.md](data-model.md)
**Format**: `[ID] [P?] [Story] Description`  ·  `[P]` = parallelizable  ·  story = US1/US2/US3

## Phase 1: Foundational (blocking prerequisites)

- [x] T001 [P] Add `ISupplierRegistry` port in `src/SddDemo.Functions/Services/ISupplierRegistry.cs` (`IsApproved(string supplier)`)
- [x] T002 Add `StaticSupplierRegistry` demo adapter in `src/SddDemo.Functions/Services/StaticSupplierRegistry.cs` — fixed in-memory `HashSet<string>` (`OrdinalIgnoreCase`)
- [x] T003 Register `ISupplierRegistry -> StaticSupplierRegistry` as singleton in `Program.cs`

## Phase 2: User Story 2 — Unknown supplier is rejected (P1) 🎯 core value

- [x] T004 [US2] Add `supplier`-approved rule to `PurchaseOrderValidator` — reject when `!supplierRegistry.IsApproved(order.Supplier)`, named `supplier`
- [x] T005 [US2] Test: `Should_Return400NamingSupplier_When_SupplierIsNotApproved`
- [x] T006 [US2] Test: `Should_Return400AndPublishNothing_When_SupplierIsNotApproved`

## Phase 3: User Story 1 — Known supplier is accepted (P1, regression guard)

- [x] T007 [US1] Test: `Should_Pass_When_SupplierIsApproved` (no regression to existing 001/002 happy-path tests)
- [x] T008 [US1] Covered by existing `Should_Return202WithCorrelationId_When_OrderIsValid` (uses an approved supplier; asserts exactly one published envelope)

## Phase 4: User Story 3 — Registry is swappable without changing callers (P3)

- [x] T009 [US3] Test: `Should_Pass_When_CustomRegistryApprovesSupplier` (construct validator with a stub `ISupplierRegistry`, no code change to validator/Function)
- [x] T010 [US3] Test: `Should_MatchSupplierCaseInsensitively_When_CasingDiffersFromRegistry`

## Phase 5: Polish

- [x] T011 [P] Add rejected-order sample request to [demo.http](../../demo.http) (unapproved supplier)
- [x] T012 [P] Contract addendum ([contracts/orders-api.md](contracts/orders-api.md)) — done as part of plan

## ADO mapping

Run `.github/prompts/tasks-to-ado.prompt.md` to turn US1/US2/US3 into Epic → Feature → Story
under the originating PROJ-100/PROJ-102 tickets. Phases 2–4 become Features; each `T0xx` becomes
a Story carrying its task id.

## Dependencies

- Phase 1 blocks Phases 2–4. T001 is `[P]`; T002 depends on T001; T003 depends on T002.
- US2 (T004–T006) is the core value and should land first; US1 regression tests (T007–T008) and
  US3 swappability tests (T009–T010) can follow once T001–T004 exist.
