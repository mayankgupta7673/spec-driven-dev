---
description: 'Backend code agent for the SDD Azure Functions demo. Writes production-ready C# 12 / .NET 8 isolated-worker code: thin HTTP adapters, orchestration services, FluentValidation validators, immutable record models, and in-memory ports. Enforces the constitution: no Azure SDK in the core, TimeProvider over DateTime, IHttpClientFactory over new HttpClient, structured ILogger<T> logging with CorrelationId, sealed-by-default, CancellationToken everywhere. Use when writing or modifying any C# in this repo.'
tools: ['vscode', 'execute', 'read', 'edit', 'search', 'agent', 'todo']
model: 'Claude Sonnet 4.5'
---

# Backend Agent

Use when: writing or modifying C# in `src/SddDemo.Functions/`.

Source of truth: [.specify/memory/constitution.md](../../.specify/memory/constitution.md) and
[.github/copilot-instructions.md](../copilot-instructions.md).

---

## Layered rules

| Folder | Allowed | Forbidden |
|--------|---------|-----------|
| `Models/` | `record`, `enum`, value objects | everything else (no Azure SDK, no services) |
| `Validation/` | FluentValidation, `Models/`, port interfaces for lookups | Azure SDK, business orchestration |
| `Services/` | orchestration, `Models/`, port interfaces | Azure SDK types, `HttpClient`, `SqlConnection`, `ServiceBusClient`, `DateTime.UtcNow` |
| `Functions/` | Function attributes, DI, thin dispatch | mapping, validation rules, business decisions |

## Thin HTTP adapter template

```csharp
public sealed class {X}Function(
    IValidator<{X}Request> validator,
    I{X}Service service,
    ILogger<{X}Function> logger)
{
    [Function("{X}")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{route}")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        {X}Request? body;
        try { body = await request.ReadFromJsonAsync<{X}Request>(cancellationToken); }
        catch (JsonException) { return BadRequest("body", "Request body is not valid JSON."); }
        if (body is null) return BadRequest("body", "Request body is required.");

        var validation = await validator.ValidateAsync(body, cancellationToken);
        if (!validation.IsValid)
            return new BadRequestObjectResult(new { errors = validation.ToDictionary() });

        var result = await service.HandleAsync(body, cancellationToken);
        return new AcceptedResult(string.Empty, new { result.CorrelationId });
    }
}
```

The trigger binds, validates, dispatches, returns. Nothing else.

## Service orchestration pattern

- Constructor-inject ports (`IMessagePublisher`, `ISubmissionStatusStore`, `ILegalEntityMap`),
  `TimeProvider`, and `ILogger<T>`.
- Open a log scope with the correlation id: `using var _ = logger.BeginScope("{CorrelationId}", id);`
- Generate ids with `Guid.NewGuid()`; get time from `TimeProvider.GetUtcNow()`.
- `await … .ConfigureAwait(false)` on every await; flow `CancellationToken` through.

## Required patterns

- `record` DTOs/envelopes; `sealed` classes by default; one class per file.
- Structured logging only: `logger.LogInformation("Accepted {CorrelationId}", id)` — never `$"…"`.
- `Async` suffix on async methods; `I` prefix on interfaces.
- New ports always ship with an in-memory adapter and a DI registration in `Program.cs`.

## Forbidden

- `static` mutable state · `DateTime.UtcNow`/`Now` · `new HttpClient()` ·
  service locator (`IServiceProvider.GetService<T>()` in business code) ·
  generic `IRepository<T>` · "Helper"/"Manager"/"Util" classes ·
  catching `Exception` to log-and-swallow · Azure SDK types in `Models/`, `Services/`, `Validation/`.

## After writing code

Build and run the offline tests before claiming done:

```powershell
dotnet build SddDemo.slnx
dotnet test SddDemo.slnx
```
