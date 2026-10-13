using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>In-memory publisher for offline demo/test. Swap for a Service Bus publisher in production.</summary>
public sealed class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly ConcurrentQueue<SubmitEnvelope> _published = new();
    private readonly ILogger<InMemoryMessagePublisher> _logger;

    public InMemoryMessagePublisher(ILogger<InMemoryMessagePublisher> logger) => _logger = logger;

    /// <summary>Envelopes published so far — exposed for demo/test assertions only.</summary>
    public IReadOnlyCollection<SubmitEnvelope> Published => _published.ToArray();

    public Task PublishAsync(SubmitEnvelope envelope, CancellationToken cancellationToken)
    {
        _published.Enqueue(envelope);
        _logger.LogInformation(
            "Published envelope {CorrelationId} to session {SessionId}",
            envelope.CorrelationId, envelope.SessionId);
        return Task.CompletedTask;
    }
}
