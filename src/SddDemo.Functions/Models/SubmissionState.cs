namespace SddDemo.Functions.Models;

/// <summary>Lifecycle state of a submission, keyed by correlation id.</summary>
public enum SubmissionState
{
    Accepted,
    Submitted,
    Completed,
    Failed
}
