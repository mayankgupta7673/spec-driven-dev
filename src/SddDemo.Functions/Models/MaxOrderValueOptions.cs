namespace SddDemo.Functions.Models;

/// <summary>Configurable ceiling on an order's total value (PROJ-101), bound via <c>IOptions</c>.</summary>
public sealed class MaxOrderValueOptions
{
    public const string SectionName = "MaxOrderValue";

    /// <summary>Maximum allowed order total (AUD). Defaults to 250,000.</summary>
    public decimal Threshold { get; set; } = 250_000m;
}
