using System.Collections.Concurrent;
using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>In-memory status store for offline demo/test. Swap for an Azure SQL (Dapper) store in production.</summary>
public sealed class InMemorySubmissionStatusStore : ISubmissionStatusStore
{
    private readonly ConcurrentDictionary<string, SubmissionStatus> _byCorrelation = new();
    private readonly ConcurrentDictionary<string, string> _correlationByClientRequest =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<string?> FindCorrelationByClientRequestAsync(string clientRequestId, CancellationToken cancellationToken)
    {
        _correlationByClientRequest.TryGetValue(clientRequestId, out var correlationId);
        return Task.FromResult(correlationId);
    }

    public Task RecordAcceptedAsync(string correlationId, string clientRequestId, DateTimeOffset at, CancellationToken cancellationToken)
    {
        _byCorrelation[correlationId] = new SubmissionStatus(correlationId, SubmissionState.Accepted, at);
        _correlationByClientRequest[clientRequestId] = correlationId;
        return Task.CompletedTask;
    }

    public Task<SubmissionStatus?> GetAsync(string correlationId, CancellationToken cancellationToken)
    {
        _byCorrelation.TryGetValue(correlationId, out var status);
        return Task.FromResult(status);
    }
}
