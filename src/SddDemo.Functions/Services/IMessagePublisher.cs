using SddDemo.Functions.Models;

namespace SddDemo.Functions.Services;

/// <summary>Publishes a submit envelope to the downstream transport (Service Bus in production).</summary>
public interface IMessagePublisher
{
    Task PublishAsync(SubmitEnvelope envelope, CancellationToken cancellationToken);
}
