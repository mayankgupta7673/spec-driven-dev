using FluentAssertions;
using Microsoft.Extensions.Options;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;
using SddDemo.Functions.Validation;

namespace SddDemo.Functions.Tests;

public sealed class PurchaseOrderValidatorTests
{
    private readonly PurchaseOrderValidator _validator = new(
        new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions()));

    private static PurchaseOrder OrderWith(
        string legalEntityId = "ABN-01",
        string supplier = "ACME Pty Ltd",
        IReadOnlyList<OrderLine>? lines = null) =>
        new(legalEntityId, supplier, "req-1", lines ?? new[] { new OrderLine("SKU-1", 10, 5.00m) });

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

    // PROJ-101: Reject purchase orders that exceed a maximum total value

    [Fact]
    public async Task Should_Return400NamingOrderTotal_When_OrderExceedsThreshold()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions { Threshold = 1000m }));
        var result = await validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 10, 150.00m) })); // total = 1500

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "orderTotal");
    }

    [Fact]
    public async Task Should_Pass_When_OrderTotalEqualsThresholdExactly()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions { Threshold = 1000m }));
        var result = await validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 10, 100.00m) })); // total = 1000

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Pass_When_OrderTotalIsBelowThreshold()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions { Threshold = 1000m }));
        var result = await validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 1, 1.00m) })); // total = 1

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_UseConfiguredThreshold_When_OptionsOverrideDefault()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions { Threshold = 50m }));
        var result = await validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 1, 100.00m) })); // total = 100, exceeds 50

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "orderTotal");
    }

    [Fact]
    public async Task Should_UseDefaultThreshold_When_NotConfigured()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StaticSupplierRegistry(), Options.Create(new MaxOrderValueOptions()));
        var result = await validator.ValidateAsync(
            OrderWith(lines: new[] { new OrderLine("SKU-1", 1, 250_000.00m) })); // total = default threshold

        result.IsValid.Should().BeTrue();
    }

    // PROJ-102: Validate supplier against an approved supplier registry

    [Fact]
    public async Task Should_Return400NamingSupplier_When_SupplierIsNotApproved()
    {
        var result = await _validator.ValidateAsync(OrderWith(supplier: "Unknown Supplier Pty Ltd"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Supplier");
    }

    [Fact]
    public async Task Should_Pass_When_SupplierIsApproved()
    {
        var result = await _validator.ValidateAsync(OrderWith(supplier: "ACME Pty Ltd"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_MatchSupplierCaseInsensitively_When_CasingDiffersFromRegistry()
    {
        var result = await _validator.ValidateAsync(OrderWith(supplier: "acme pty ltd"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Pass_When_CustomRegistryApprovesSupplier()
    {
        var validator = new PurchaseOrderValidator(
            new StaticLegalEntityMap(), new StubSupplierRegistry("Custom Supplier"), Options.Create(new MaxOrderValueOptions()));
        var result = await validator.ValidateAsync(OrderWith(supplier: "Custom Supplier"));

        result.IsValid.Should().BeTrue();
    }

    private sealed class StubSupplierRegistry : ISupplierRegistry
    {
        private readonly string _approved;

        public StubSupplierRegistry(string approved) => _approved = approved;

        public bool IsApproved(string supplier) =>
            string.Equals(supplier, _approved, StringComparison.OrdinalIgnoreCase);
    }
}
