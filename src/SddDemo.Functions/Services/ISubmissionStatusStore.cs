using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>Stores and retrieves submission status, with idempotency lookup by client request id.</summary>
public interface ISubmissionStatusStore
{
    Task<string?> FindCorrelationByClientRequestAsync(string clientRequestId, CancellationToken cancellationToken);

    Task RecordAcceptedAsync(string correlationId, string clientRequestId, DateTimeOffset at, CancellationToken cancellationToken);

    Task<SubmissionStatus?> GetAsync(string correlationId, CancellationToken cancellationToken);
}
