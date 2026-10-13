using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SddDemo.Functions.Services;

namespace SddDemo.Functions.Functions;

/// <summary>HTTP adapter: reads submission status by correlation id.</summary>
public sealed class GetStatusFunction
{
    private readonly ISubmissionService _submissionService;

    public GetStatusFunction(ISubmissionService submissionService) =>
        _submissionService = submissionService;

    [Function("GetStatus")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{correlationId}/status")] HttpRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var status = await _submissionService.GetStatusAsync(correlationId, cancellationToken).ConfigureAwait(false);
        return status is null
            ? new NotFoundResult()
            : new OkObjectResult(new
            {
                status.CorrelationId,
                State = status.State.ToString(),
                status.LastUpdated
            });
    }
}
