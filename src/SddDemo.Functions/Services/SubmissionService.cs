using Microsoft.Extensions.Logging;
using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>Core submission workflow. Pure orchestration — no Azure SDK types.</summary>
public sealed class SubmissionService : ISubmissionService
{
    private readonly IMessagePublisher _publisher;
    private readonly ISubmissionStatusStore _statusStore;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        IMessagePublisher publisher,
        ISubmissionStatusStore statusStore,
        TimeProvider timeProvider,
        ILogger<SubmissionService> logger)
    {
        _publisher = publisher;
        _statusStore = statusStore;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<SubmissionResult> SubmitAsync(PurchaseOrder order, CancellationToken cancellationToken)
    {
        var existing = await _statusStore
            .FindCorrelationByClientRequestAsync(order.ClientRequestId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent replay for client request {ClientRequestId}; returning {CorrelationId}",
                order.ClientRequestId, existing);
            return new SubmissionResult(existing, WasDuplicate: true);
        }

        var correlationId = Guid.NewGuid().ToString("N");
        using var scope = _logger.BeginScope("{CorrelationId}", correlationId);

        var envelope = new SubmitEnvelope(correlationId, order.LegalEntityId, order);
        await _publisher.PublishAsync(envelope, cancellationToken).ConfigureAwait(false);

        await _statusStore
            .RecordAcceptedAsync(correlationId, order.ClientRequestId, _timeProvider.GetUtcNow(), cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Accepted order for {LegalEntityId} with {LineCount} line(s)",
            order.LegalEntityId, order.Lines.Count);

        return new SubmissionResult(correlationId, WasDuplicate: false);
    }

    public Task<SubmissionStatus?> GetStatusAsync(string correlationId, CancellationToken cancellationToken) =>
        _statusStore.GetAsync(correlationId, cancellationToken);
}
