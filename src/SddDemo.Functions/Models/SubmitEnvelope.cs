namespace SddDemo.Functions.Models;

/// <summary>Typed message published to the submit topic for one accepted order.</summary>
public sealed record SubmitEnvelope(
    string CorrelationId,
    string SessionId,
    PurchaseOrder Order);
