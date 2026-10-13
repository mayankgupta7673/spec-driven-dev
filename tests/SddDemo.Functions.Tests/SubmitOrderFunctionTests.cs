using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using SddDemo.Functions.Functions;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;
using SddDemo.Functions.Validation;

namespace SddDemo.Functions.Tests;

public sealed class SubmitOrderFunctionTests
{
    private readonly InMemoryMessagePublisher _publisher = new(NullLogger<InMemoryMessagePublisher>.Instance);
    private readonly InMemorySubmissionStatusStore _store = new();
    private readonly FakeTimeProvider _time = new(DateTimeOffset.Parse("2026-10-13T00:00:00Z"));

    private SubmitOrderFunction CreateSut()
    {
        var service = new SubmissionService(_publisher, _store, _time, NullLogger<SubmissionService>.Instance);
        var validator = new PurchaseOrderValidator(new StaticLegalEntityMap());
        return new SubmitOrderFunction(validator, service, NullLogger<SubmitOrderFunction>.Instance);
    }

    private static HttpRequest JsonRequest(object body)
    {
        var json = JsonSerializer.Serialize(body);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = Encoding.UTF8.GetByteCount(json);
        return context.Request;
    }

    [Fact]
    public async Task Should_Return202WithCorrelationId_When_OrderIsValid()
    {
        var sut = CreateSut();
        var request = JsonRequest(new
        {
            legalEntityId = "ABN-01",
            supplier = "ACME Pty Ltd",
            clientRequestId = "req-100",
            lines = new[] { new { sku = "SKU-1", quantity = 10, unitPrice = 5.00m } }
        });

        var result = await sut.RunAsync(request, CancellationToken.None);

        var accepted = result.Should().BeOfType<AcceptedResult>().Subject;
        accepted.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        accepted.Value.Should().NotBeNull();
        _publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_Return400AndPublishNothing_When_OrderHasNoLines()
    {
        var sut = CreateSut();
        var request = JsonRequest(new
        {
            legalEntityId = "ABN-01",
            supplier = "ACME Pty Ltd",
            clientRequestId = "req-101",
            lines = Array.Empty<object>()
        });

        var result = await sut.RunAsync(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        _publisher.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return400_When_BodyIsNotJson()
    {
        var sut = CreateSut();
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("not-json"));
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = 8;

        var result = await sut.RunAsync(context.Request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        _publisher.Published.Should().BeEmpty();
    }
}

public sealed class GetStatusFunctionTests
{
    private readonly InMemoryMessagePublisher _publisher = new(NullLogger<InMemoryMessagePublisher>.Instance);
    private readonly InMemorySubmissionStatusStore _store = new();
    private readonly FakeTimeProvider _time = new(DateTimeOffset.Parse("2026-10-13T00:00:00Z"));

    private (GetStatusFunction Function, SubmissionService Service) CreateSut()
    {
        var service = new SubmissionService(_publisher, _store, _time, NullLogger<SubmissionService>.Instance);
        return (new GetStatusFunction(service), service);
    }

    [Fact]
    public async Task Should_ReturnStatus_When_CorrelationExists()
    {
        var (function, service) = CreateSut();
        var order = new PurchaseOrder("ABN-01", "ACME Pty Ltd", "req-200",
            new[] { new OrderLine("SKU-1", 10, 5.00m) });
        var accepted = await service.SubmitAsync(order, CancellationToken.None);

        var result = await function.RunAsync(new DefaultHttpContext().Request, accepted.CorrelationId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Should_Return404_When_Unknown()
    {
        var (function, _) = CreateSut();

        var result = await function.RunAsync(new DefaultHttpContext().Request, "unknown", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
