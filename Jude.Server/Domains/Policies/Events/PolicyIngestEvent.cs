namespace Jude.Server.Domains.Policies.Events;

public record PolicyIngestEvent(
    int PolicyId,
    string PolicyName,
    byte[] FileContent,
    string FileName,
    DateTime IngestedAt
);
