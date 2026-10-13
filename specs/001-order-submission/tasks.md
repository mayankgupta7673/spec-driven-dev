# Tasks: Asynchronous Purchase Order Submission

**Input**: [spec.md](spec.md), [plan.md](plan.md)
**Format**: `[ID] [P?] [Story] Description`  ·  `[P]` = parallelizable  ·  story = US1/US2/US3

## Phase 1: Setup

- [x] T001 Scaffold .NET 8 isolated Functions app + xUnit test project (done in repo setup)

## Phase 2: Foundational (blocking prerequisites)

- [x] T002 [P] Add Models: `PurchaseOrder`, `OrderLine`, `SubmitEnvelope`, `SubmissionState`, `SubmissionStatus`, `SubmissionResult` in `src/SddDemo.Functions/Models/`
- [x] T003 [P] Add `ILegalEntityMap` + `StaticLegalEntityMap` in `src/SddDemo.Functions/Services/`
- [x] T004 [P] Add `IMessagePublisher` + `InMemoryMessagePublisher` in `src/SddDemo.Functions/Services/`
- [x] T005 [P] Add `ISubmissionStatusStore` + `InMemorySubmissionStatusStore` in `src/SddDemo.Functions/Services/`
- [x] T006 Wire DI in `Program.cs` (FluentValidation, TimeProvider, services as singletons)

## Phase 3: User Story 1 — Submit & acknowledge (P1) 🎯 MVP

- [x] T007 [US1] Implement `ISubmissionService.SubmitAsync` — correlation id, session id, publish, record Accepted
- [x] T008 [US1] Implement `SubmitOrderFunction` (HTTP POST `/api/orders`) — bind, validate, dispatch, return 202
- [x] T009 [US1] Test: `Should_Return202WithCorrelationId_When_OrderIsValid`
- [x] T010 [US1] Test: `Should_PublishExactlyOneEnvelopeWithLegalEntitySession_When_Accepted`

## Phase 4: User Story 2 — Reject invalid early (P2)

- [x] T011 [US2] Implement `PurchaseOrderValidator` (FluentValidation) — lines required, legal-entity known
- [x] T012 [US2] Test: `Should_Return400AndPublishNothing_When_OrderHasNoLines`
- [x] T013 [US2] Test: `Should_FailValidation_When_LegalEntityUnknown`

## Phase 5: User Story 3 — Query status (P3)

- [x] T014 [US3] Implement `GetStatusFunction` (HTTP GET `/api/orders/{correlationId}/status`)
- [x] T015 [US3] Test: `Should_ReturnStatus_When_CorrelationExists` and `Should_Return404_When_Unknown`

## Phase 6: Idempotency & polish

- [x] T016 Implement idempotency on `clientRequestId` in `SubmissionService` + store
- [x] T017 [P] Test: `Should_ReturnSameCorrelationId_And_PublishOnce_When_SubmittedTwice`
- [x] T018 [P] Add `demo.http` sample requests + README run steps

## ADO mapping

Run `.github/prompts/tasks-to-ado.prompt.md` to turn US1/US2/US3 into Epic → Feature → Story.
Phases 3/4/5 become Features; each `T0xx` becomes a Story carrying its task id.

## Dependencies

- Phase 2 blocks Phases 3–6. Within Phase 2, T002–T005 are `[P]`. T006 after T002–T005.
- US1 (T007–T010) is the MVP and can ship alone.
