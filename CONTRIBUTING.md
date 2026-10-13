# Contributing

How to add a feature to this repository. The method is not optional — see
[ADR-0001](docs/adr/ADR-0001-spec-driven-development.md).

## Add a feature (the loop)

1. **Seed the spec from the backlog.**
   - JIRA: run the `jira-to-spec` prompt with the issue key.
   - ADO: run the `ado-to-spec` prompt with the work-item id.
   - Net-new (no ticket): run `/speckit.specify`.
2. `/speckit.clarify` — resolve `[NEEDS CLARIFICATION]` items.
3. `/speckit.plan` — produce `plan.md`, `contracts/`, `data-model.md`.
4. `/speckit.tasks` — produce `tasks.md`; then `tasks-to-ado` to create child work items.
5. `/speckit.analyze` — consistency gate (optional).
6. `/speckit.implement` — write the vertical slice and tests.
7. `/speckit.converge` — repeat implement→converge until it reports Converged.

## Vertical-slice checklist (self-check before PR)

- [ ] A `spec.md` exists and the change traces to it (and to a ticket id).
- [ ] Function triggers are thin adapters; logic is in `Services/` / `Validation/`.
- [ ] No Azure SDK references in `Models/` or `Services/`; tests run offline.
- [ ] No `static` mutable state, no `DateTime.UtcNow` (inject `TimeProvider`), no `new HttpClient()`.
- [ ] All async methods accept and pass a `CancellationToken`.
- [ ] Structured logging with `{CorrelationId}`; no string-interpolated log templates.
- [ ] New types are `sealed` unless inheritance is intended; DTOs are `record`s.
- [ ] Tests cover the new logic; `dotnet test SddDemo.slnx` is green.
- [ ] The PR references its ticket id.

## Build, test, run

```powershell
dotnet build SddDemo.slnx
dotnet test SddDemo.slnx
cd src/SddDemo.Functions; func start      # then use demo.http
```

## Conventions

- One class per file; filename matches the type; folder matches the namespace.
- `Async` suffix on async methods; `I` prefix on interfaces; `Should_X_When_Y` test names.
- Keep PRs small (< 400 LOC) and releasable. Editing shared guardrails needs an ADR.
