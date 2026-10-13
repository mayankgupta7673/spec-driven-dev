namespace SddDemo.Functions.Models;

/// <summary>Latest known status for a submission.</summary>
public sealed record SubmissionStatus(
    string CorrelationId,
    SubmissionState State,
    DateTimeOffset LastUpdated);
