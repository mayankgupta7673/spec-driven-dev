using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>Orchestrates acceptance of a validated order: idempotency, publish, status record.</summary>
public interface ISubmissionService
{
    Task<SubmissionResult> SubmitAsync(PurchaseOrder order, CancellationToken cancellationToken);

    Task<SubmissionStatus?> GetStatusAsync(string correlationId, CancellationToken cancellationToken);
}
