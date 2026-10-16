namespace SddDemo.Functions.Models;

/// <summary>Derived calculations over a <see cref="PurchaseOrder"/> — not persisted.</summary>
public static class PurchaseOrderExtensions
{
    /// <summary>The order's total value: the sum of quantity × unitPrice across all lines.</summary>
    public static decimal GetOrderTotal(this PurchaseOrder order) =>
        order.Lines.Sum(line => line.Quantity * line.UnitPrice);
}
