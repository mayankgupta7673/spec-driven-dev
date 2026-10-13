namespace SddDemo.Functions.Models;

/// <summary>Inbound purchase order contract submitted by an upstream caller.</summary>
public sealed record PurchaseOrder(
    string LegalEntityId,
    string Supplier,
    string ClientRequestId,
    IReadOnlyList<OrderLine> Lines);
