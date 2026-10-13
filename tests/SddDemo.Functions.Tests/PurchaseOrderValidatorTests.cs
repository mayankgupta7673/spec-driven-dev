using FluentAssertions;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;
using SddDemo.Functions.Validation;

namespace SddDemo.Functions.Tests;

public sealed class PurchaseOrderValidatorTests
{
    private readonly PurchaseOrderValidator _validator = new(new StaticLegalEntityMap());

    private static PurchaseOrder OrderWith(
        string legalEntityId = "ABN-01",
        IReadOnlyList<OrderLine>? lines = null) =>
        new(legalEntityId, "ACME Pty Ltd", "req-1", lines ?? new[] { new OrderLine("SKU-1", 10, 5.00m) });

    [Fact]
    public async Task Should_Pass_When_OrderIsValid()
    {
        var result = await _validator.ValidateAsync(OrderWith());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_FailNamingLines_When_OrderHasNoLines()
    {
        var result = await _validator.ValidateAsync(OrderWith(lines: Array.Empty<OrderLine>()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Lines");
    }

    [Fact]
    public async Task Should_FailNamingLegalEntity_When_LegalEntityUnknown()
    {
        var result = await _validator.ValidateAsync(OrderWith(legalEntityId: "UNKNOWN-99"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LegalEntityId");
    }

    [Fact]
    public async Task Should_Fail_When_LineQuantityIsZero()
    {
        var result = await _validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 0, 5.00m) }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }
}
