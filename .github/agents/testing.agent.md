---
description: 'Testing agent for the SDD Azure Functions demo. Writes and enforces offline-first tests with xUnit + FluentAssertions + NSubstitute, plus function-level tests that exercise HTTP triggers via DefaultHttpContext. Enforces Should_X_When_Y naming, Arrange-Act-Assert, FakeTimeProvider over Thread.Sleep, and no random data. Use when writing or modifying tests.'
tools: ['vscode', 'execute', 'read', 'edit', 'search', 'agent', 'todo']
model: 'Claude Sonnet 4.5'
---

# Testing Agent

Use when: writing or modifying tests in `tests/SddDemo.Functions.Tests`.

Source of truth: [.specify/memory/constitution.md](../../.specify/memory/constitution.md) §III.

---

## The rule: offline first

Every test runs with `dotnet test` on a laptop with **no internet, no Docker, no Azure login**.
If a test needs any of those, it is in the wrong layer — redesign around a port + in-memory adapter.

| Level | Tooling | Runs offline? |
|-------|---------|---------------|
| Unit (service, validator, mapper) | xUnit, FluentAssertions, NSubstitute | **Yes** |
| Function-level (trigger via `DefaultHttpContext`) | xUnit + ASP.NET Core test host types | **Yes** |
| Integration (future: SB/SQL) | Testcontainers | Yes (Docker) |

## Unit test rules

- One `[Fact]` per behaviour. No "test the whole flow" mega-facts.
- Naming: `Should_X_When_Y` or `Method_Should_X_When_Y`.
- Arrange / Act / Assert separated by blank lines.
- One assertion concept per test (all properties of one record is fine).
- **No `Thread.Sleep`.** Use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`.
- No random data — fixed, readable fixtures. `TheoryData<>` for parameterised cases.
- Prefer per-test instances; `IClassFixture<>` only for genuinely shared read-only setup.

## Service test pattern

```csharp
public sealed class SubmissionServiceTests
{
    private readonly InMemoryMessagePublisher _publisher = new(NullLogger<InMemoryMessagePublisher>.Instance);
    private readonly InMemorySubmissionStatusStore _store = new();
    private readonly FakeTimeProvider _time = new(DateTimeOffset.Parse("2026-10-13T00:00:00Z"));

    private SubmissionService CreateSut() =>
        new(_publisher, _store, _time, NullLogger<SubmissionService>.Instance);

    [Fact]
    public async Task Should_PublishExactlyOneEnvelope_When_Accepted()
    {
        var sut = CreateSut();

        await sut.SubmitAsync(ValidOrder(), CancellationToken.None);

        _publisher.Published.Should().ContainSingle();
    }
}
```

## Function-level test pattern (HTTP trigger, offline)

```csharp
private static HttpRequest JsonRequest(object body)
{
    var json = JsonSerializer.Serialize(body);
    var ctx = new DefaultHttpContext();
    ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
    ctx.Request.ContentType = "application/json";
    ctx.Request.ContentLength = Encoding.UTF8.GetByteCount(json);
    return ctx.Request;
}

// assert on IActionResult types: AcceptedResult (202), BadRequestObjectResult (400),
// NotFoundResult (404), OkObjectResult (200).
```

## What to cover for a new feature

- **Validator**: each rule — valid passes; each invalid case names the offending field.
- **Service**: happy path, idempotency, status recorded, unknown-id returns null.
- **Trigger**: 202 on valid, 400 on invalid + nothing published, 404 on unknown, 400 on bad JSON.

## Forbidden

- `Thread.Sleep` · random/`Guid.NewGuid()` as test input · `[Fact(Skip="…")]` without an issue link ·
  network, Docker, or Azure in a unit test · asserting on log output instead of behaviour.

## After writing tests

```powershell
dotnet test SddDemo.slnx
```
