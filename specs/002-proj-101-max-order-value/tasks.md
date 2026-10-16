# Tasks: Reject Purchase Orders Exceeding a Maximum Total Value

**Input**: [spec.md](spec.md), [plan.md](plan.md), [data-model.md](data-model.md)
**Format**: `[ID] [P?] [Story] Description`  ·  `[P]` = parallelizable  ·  story = US1/US2/US3

## Phase 1: Foundational (blocking prerequisites)

- [x] T001 [P] Add `MaxOrderValueOptions` record in `src/SddDemo.Functions/Models/MaxOrderValueOptions.cs` (`Threshold`, default `250_000m`)
- [x] T002 [P] Add `PurchaseOrderExtensions.GetOrderTotal()` in `src/SddDemo.Functions/Models/PurchaseOrderExtensions.cs`
- [x] T003 Wire `IOptions<MaxOrderValueOptions>` in `Program.cs` — bind from config section `MaxOrderValue`, `ValidateDataAnnotations`/`Validate(o => o.Threshold > 0)`, `.ValidateOnStart()`

## Phase 2: User Story 2 — Order above the limit is rejected (P1) 🎯 core value

- [x] T004 [US2] Add `orderTotal` rule to `PurchaseOrderValidator` — reject when `GetOrderTotal() > options.Threshold`, named `orderTotal`
- [x] T005 [US2] Test: `Should_Return400NamingOrderTotal_When_OrderExceedsThreshold`
- [x] T006 [US2] Test: `Should_PublishNothing_When_OrderExceedsThreshold`

## Phase 3: User Story 1 — Order within the limit is accepted (P1, regression guard)

- [x] T007 [US1] Test: `Should_Return202_When_OrderTotalEqualsThresholdExactly`
- [x] T008 [US1] Test: `Should_Return202_When_OrderTotalIsBelowThreshold` (no regression to existing 001 happy-path tests)

## Phase 4: User Story 3 — Threshold is configurable per environment (P2)

- [x] T009 [US3] Test: `Should_UseConfiguredThreshold_When_OptionsOverrideDefault` (construct validator with a custom `IOptions<MaxOrderValueOptions>`, no code change)
- [x] T010 [US3] Test: `Should_UseDefaultThreshold_When_NotConfigured` (default 250,000)
- [x] T011 [US3] Test: `Should_FailFastAtStartup_When_ThresholdIsNotPositive`

## Phase 5: Polish

- [x] T012 [P] Add rejected-order sample request to [demo.http](../../demo.http) (order exceeding threshold)
- [x] T013 [P] Update [contracts/orders-api.md](contracts/orders-api.md) addendum (done as part of plan)

## ADO mapping

Run `.github/prompts/tasks-to-ado.prompt.md` to turn US1/US2/US3 into Epic → Feature → Story
under the originating PROJ-100/PROJ-101 tickets. Phases 2–4 become Features; each `T0xx` becomes
a Story carrying its task id.

## Dependencies

- Phase 1 blocks Phases 2–4. T001–T002 are `[P]`; T003 depends on T001.
- US2 (T004–T006) is the core value and should land first; US1 regression tests (T007–T008) and
  US3 configurability tests (T009–T011) can follow in parallel once T001–T004 exist.

## Phase 6: Convergence

- [ ] T014 Log the computed `orderTotal` value (not just the error count) when a `SubmitOrderFunction` rejection includes the `orderTotal` field, per FR-006 (partial)

