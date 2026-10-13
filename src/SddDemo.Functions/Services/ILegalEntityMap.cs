namespace SddDemo.Functions.Services;

/// <summary>Resolves whether a legal-entity id is mapped to a known D365 company.</summary>
public interface ILegalEntityMap
{
    bool IsKnown(string legalEntityId);
}
