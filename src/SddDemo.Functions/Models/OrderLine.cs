namespace SddDemo.Functions.Models;

/// <summary>A single line on a purchase order.</summary>
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice);
