namespace ProjectUpdateCloner.Models;

public sealed record CopyResult(
    string DestinationFolder,
    string? ZipPath,
    int CopiedCount,
    int ErrorCount);
