using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;

namespace SddDemo.Functions.Functions;

/// <summary>HTTP adapter: bind, validate, dispatch, return. No business logic here.</summary>
public sealed class SubmitOrderFunction
{
    private readonly IValidator<PurchaseOrder> _validator;
    private readonly ISubmissionService _submissionService;
    private readonly ILogger<SubmitOrderFunction> _logger;

    public SubmitOrderFunction(
        IValidator<PurchaseOrder> validator,
        ISubmissionService submissionService,
        ILogger<SubmitOrderFunction> logger)
    {
        _validator = validator;
        _submissionService = submissionService;
        _logger = logger;
    }

    [Function("SubmitOrder")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        PurchaseOrder? order;
        try
        {
            order = await request.ReadFromJsonAsync<PurchaseOrder>(cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return BadRequest("body", "Request body is not valid JSON.");
        }

        if (order is null)
        {
            return BadRequest("body", "Request body is required.");
        }

        var validation = await _validator.ValidateAsync(order, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            _logger.LogInformation("Rejected order for {LegalEntityId}: {ErrorCount} error(s)",
                order.LegalEntityId, validation.Errors.Count);
            return new BadRequestObjectResult(new { errors });
        }

        var result = await _submissionService.SubmitAsync(order, cancellationToken).ConfigureAwait(false);
        return new AcceptedResult(string.Empty, new { correlationId = result.CorrelationId });
    }

    private static BadRequestObjectResult BadRequest(string field, string message) =>
        new(new { errors = new Dictionary<string, string[]> { [field] = new[] { message } } });
}
