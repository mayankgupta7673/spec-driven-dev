using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;

namespace SddDemo.Functions.Tests;

public sealed class SubmissionServiceTests
{
    private readonly InMemoryMessagePublisher _publisher = new(NullLogger<InMemoryMessagePublisher>.Instance);
    private readonly InMemorySubmissionStatusStore _store = new();
    private readonly FakeTimeProvider _time = new(DateTimeOffset.Parse("2026-10-13T00:00:00Z"));

    private SubmissionService CreateSut() =>
        new(_publisher, _store, _time, NullLogger<SubmissionService>.Instance);

    private static PurchaseOrder ValidOrder(string clientRequestId = "req-1") =>
        new("ABN-01", "ACME Pty Ltd", clientRequestId, new[] { new OrderLine("SKU-1", 10, 5.00m) });

    [Fact]
    public async Task Should_ReturnCorrelationId_When_OrderIsValid()
    {
        var sut = CreateSut();

        var result = await sut.SubmitAsync(ValidOrder(), CancellationToken.None);

        result.CorrelationId.Should().NotBeNullOrWhiteSpace();
        result.WasDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task Should_PublishExactlyOneEnvelopeWithLegalEntitySession_When_Accepted()
    {
        var sut = CreateSut();

        var result = await sut.SubmitAsync(ValidOrder(), CancellationToken.None);

        _publisher.Published.Should().ContainSingle();
        var envelope = _publisher.Published.Single();
        envelope.CorrelationId.Should().Be(result.CorrelationId);
        envelope.SessionId.Should().Be("ABN-01");
    }

    [Fact]
    public async Task Should_RecordAcceptedStatus_When_Accepted()
    {
        var sut = CreateSut();

        var result = await sut.SubmitAsync(ValidOrder(), CancellationToken.None);
        var status = await sut.GetStatusAsync(result.CorrelationId, CancellationToken.None);

        status.Should().NotBeNull();
        status!.State.Should().Be(SubmissionState.Accepted);
        status.LastUpdated.Should().Be(_time.GetUtcNow());
    }

    [Fact]
    public async Task Should_ReturnSameCorrelationId_And_PublishOnce_When_SubmittedTwice()
    {
        var sut = CreateSut();

        var first = await sut.SubmitAsync(ValidOrder("req-dup"), CancellationToken.None);
        var second = await sut.SubmitAsync(ValidOrder("req-dup"), CancellationToken.None);

        second.CorrelationId.Should().Be(first.CorrelationId);
        second.WasDuplicate.Should().BeTrue();
        _publisher.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_ReturnNullStatus_When_CorrelationIsUnknown()
    {
        var sut = CreateSut();

        var status = await sut.GetStatusAsync("does-not-exist", CancellationToken.None);

        status.Should().BeNull();
    }
}
