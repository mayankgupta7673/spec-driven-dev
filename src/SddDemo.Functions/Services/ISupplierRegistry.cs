namespace SddDemo.Functions.Services;

/// <summary>Resolves whether a supplier name is an approved supplier.</summary>
public interface ISupplierRegistry
{
    bool IsApproved(string supplier);
}
