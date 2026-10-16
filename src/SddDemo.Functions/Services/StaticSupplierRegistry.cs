namespace SddDemo.Functions.Services;

/// <summary>Demo supplier registry. Replace with a config- or SQL-backed registry in production.</summary>
public sealed class StaticSupplierRegistry : ISupplierRegistry
{
    private static readonly HashSet<string> Approved =
        new(StringComparer.OrdinalIgnoreCase) { "ACME Pty Ltd", "Bunnings Trade", "Reece Civil Supplies" };

    public bool IsApproved(string supplier) =>
        !string.IsNullOrWhiteSpace(supplier) && Approved.Contains(supplier);
}
