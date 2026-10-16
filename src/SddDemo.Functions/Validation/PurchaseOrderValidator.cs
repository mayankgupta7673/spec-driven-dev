using FluentValidation;
using Microsoft.Extensions.Options;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;

namespace SddDemo.Functions.Validation;

/// <summary>Boundary validation for an inbound purchase order.</summary>
public sealed class PurchaseOrderValidator : AbstractValidator<PurchaseOrder>
{
    public PurchaseOrderValidator(
        ILegalEntityMap legalEntityMap,
        ISupplierRegistry supplierRegistry,
        IOptions<MaxOrderValueOptions> maxOrderValueOptions)
    {
        var threshold = maxOrderValueOptions.Value.Threshold;

        RuleFor(o => o.ClientRequestId)
            .NotEmpty().WithMessage("clientRequestId is required.");

        RuleFor(o => o.Supplier)
            .NotEmpty().WithMessage("supplier is required.")
            .Must(supplierRegistry.IsApproved).WithMessage("supplier is not an approved supplier.");

        RuleFor(o => o.LegalEntityId)
            .NotEmpty().WithMessage("legalEntityId is required.")
            .Must(legalEntityMap.IsKnown).WithMessage("legalEntityId is not a known legal entity.");

        RuleFor(o => o.Lines)
            .NotEmpty().WithMessage("lines is required and must contain at least one line.");

        RuleForEach(o => o.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Sku).NotEmpty().WithMessage("sku is required.");
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("quantity must be greater than zero.");
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("unitPrice must be zero or greater.");
        });

        RuleFor(o => o)
            .Must(o => o.GetOrderTotal() <= threshold)
            .WithName("orderTotal")
            .WithMessage(o => $"orderTotal {o.GetOrderTotal():F2} exceeds the maximum allowed value of {threshold:F2}.")
            .When(o => o.Lines.Count > 0);
    }
}

