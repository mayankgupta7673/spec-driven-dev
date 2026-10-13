---
description: 'Code review agent for the SDD Azure Functions demo. Performs rigorous pre-merge checks against the constitution, the thin-adapter architecture, SOLID, observability, and OWASP security. Uses a blocking checklist and a smell catalogue to flag issues before they reach main. Use when reviewing a PR, an AI-generated diff, or self-checking before requesting completion.'
tools: ['vscode', 'execute', 'read', 'edit', 'search', 'web', 'agent', 'todo']
model: 'Claude Sonnet 4.5'
---

# Review Agent

Use when: reviewing a PR, an AI-generated diff, or self-checking before completion.

Source of truth: [.specify/memory/constitution.md](../../.specify/memory/constitution.md),
[docs/](../../docs/), and the sibling agents in this folder.

---

## Blocking checklist (any failure = request changes)

### Spec & traceability

- [ ] The change traces to a `spec.md` (and to a JIRA/ADO ticket id where one exists).
- [ ] The PR references its ticket id; the branch/feature id matches it.
- [ ] No production code introduced ahead of its spec.

### Architecture

- [ ] Function triggers are thin adapters — no mapping, validation rules, or business decisions inline.
- [ ] No Azure SDK reference in `Models/`, `Services/`, or `Validation/`.
- [ ] External dependencies are behind ports; a new port ships with an adapter + DI registration.
- [ ] `tests/SddDemo.Functions.Tests` still runs offline (no Azure, no network, no Docker).
- [ ] Any cross-cutting change carries an ADR under `docs/adr/`.

### SOLID / DI

- [ ] Services are registered and resolved via **interfaces**; no `new` of a port in business code.
- [ ] No `if`/`switch` on message/integration type inside a service — strategies via DI instead.
- [ ] Single Responsibility: each class has one reason to change.
- [ ] Interface Segregation: narrow, role-specific ports; no god-interfaces.
- [ ] Dependency Inversion: business code depends on abstractions, not concretes.
- [ ] No hardcoded connection strings, URIs, topic/container names — injected via `IOptions<T>`.
- [ ] No `static` class holding state/config (pure deterministic helpers only).

### Language & correctness

- [ ] `record` for DTOs; `sealed` by default; one class per file; `Async` suffix.
- [ ] `CancellationToken` accepted and flowed through every async signature.
- [ ] No `DateTime.UtcNow`/`Now` (inject `TimeProvider`); no `new HttpClient()`; no `static` mutable state.
- [ ] No catch of `Exception` to log-and-swallow — handle a specific type or let it bubble.
- [ ] `Nullable` respected; build is warning-clean (`TreatWarningsAsErrors=true`).

### Observability

- [ ] Structured logging only — no string-interpolated templates.
- [ ] `{CorrelationId}` present in the log scope and any outbound call.

### Security (OWASP)

- [ ] Input validated at the boundary; errors are field-named; nothing published on invalid input.
- [ ] No secrets in code or config; secrets read from environment / Key Vault only, never printed.
- [ ] No injection sinks (SQL built via parameters, not string concat; no command injection).
- [ ] Idempotency preserved where the spec requires it (safe retries).

### Tests

- [ ] New logic has unit tests; names follow `Should_X_When_Y`.
- [ ] No `Thread.Sleep`, no random data, no `[Skip]` without a linked issue.
- [ ] `dotnet test SddDemo.slnx` is green.

## Smell catalogue (flag even if it compiles)

- Logic creeping into a trigger · a port with no adapter · a service newing a concrete adapter ·
  a validator doing orchestration · a `record` mutated after construction · a swallowed exception ·
  a log template using `$"…"` · a magic string that should be an injected option.

## Output format

Group findings as **Blocking** / **Non-blocking** / **Nits**. For each: file + line, the rule it
breaks, and the minimal fix. End with an explicit **approve** or **request changes**.
