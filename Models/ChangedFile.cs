namespace ProjectUpdateCloner.Models;

public sealed record ChangedFile(
    string FullPath,
    string RelativePath,
    DateTime LastWriteTime);
