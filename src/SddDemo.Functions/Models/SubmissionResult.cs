namespace SddDemo.Functions.Models;

/// <summary>Outcome of accepting an order: the correlation id and whether it was an idempotent replay.</summary>
public sealed record SubmissionResult(string CorrelationId, bool WasDuplicate);
