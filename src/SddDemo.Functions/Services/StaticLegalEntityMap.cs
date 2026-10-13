namespace SddDemo.Functions.Services;

/// <summary>Demo legal-entity map. Replace with a config- or SQL-backed map in production.</summary>
public sealed class StaticLegalEntityMap : ILegalEntityMap
{
    private static readonly HashSet<string> Known =
        new(StringComparer.OrdinalIgnoreCase) { "ABN-01", "ABN-02", "ABN-03" };

    public bool IsKnown(string legalEntityId) =>
        !string.IsNullOrWhiteSpace(legalEntityId) && Known.Contains(legalEntityId);
}
