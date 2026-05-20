namespace ProjectUpdateCloner.Models;

public sealed record ScanOptions
{
    public required string SourceFolder { get; init; }

    public required DateTime SelectedDate { get; init; }

    public bool UseCommonIgnores { get; init; } = true;

    public bool IncludeHiddenFiles { get; init; }

    public string FilePatterns { get; init; } = string.Empty;

    public IReadOnlyList<string> IgnoredRelativeFolders { get; init; } = [];
}
