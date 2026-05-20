using System.IO;
using System.Text.RegularExpressions;
using ProjectUpdateCloner.Models;

namespace ProjectUpdateCloner.Services;

public sealed class FileScannerService
{
    private static readonly string[] CommonIgnoredFolders =
    [
        ".git",
        "bin",
        "obj",
        "vendor",
        "node_modules",
        Path.Combine("storage", "logs")
    ];

    public IReadOnlyList<ChangedFile> Scan(ScanOptions options, Action<LogEntry> log)
    {
        var sourceFolder = Path.GetFullPath(options.SourceFolder);
        var selectedDate = options.SelectedDate.Date;
        var patterns = ParsePatterns(options.FilePatterns);
        var results = new List<ChangedFile>();

        log(new LogEntry(LogLevel.Info, $"Scanning {sourceFolder}"));

        var ignoredFolders = NormalizeIgnoredFolders(options.IgnoredRelativeFolders);

        foreach (var filePath in EnumerateFiles(sourceFolder, options, ignoredFolders, log))
        {
            try
            {
                var relativePath = Path.GetRelativePath(sourceFolder, filePath);

                if (IsIgnoredPath(relativePath, options.UseCommonIgnores, ignoredFolders))
                {
                    log(new LogEntry(LogLevel.Skip, relativePath));
                    continue;
                }

                var attributes = File.GetAttributes(filePath);
                if (!options.IncludeHiddenFiles && attributes.HasFlag(FileAttributes.Hidden))
                {
                    log(new LogEntry(LogLevel.Skip, relativePath));
                    continue;
                }

                if (patterns.Count > 0 && !MatchesAnyPattern(relativePath, patterns))
                {
                    log(new LogEntry(LogLevel.Skip, relativePath));
                    continue;
                }

                var lastWriteTime = File.GetLastWriteTime(filePath);
                if (lastWriteTime.Date == selectedDate)
                {
                    results.Add(new ChangedFile(filePath, relativePath, lastWriteTime));
                    log(new LogEntry(LogLevel.Success, $"Matched {relativePath}"));
                }
            }
            catch (Exception ex)
            {
                log(new LogEntry(LogLevel.Error, $"{filePath}: {ex.Message}"));
            }
        }

        return results
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<string> EnumerateFiles(
        string sourceFolder,
        ScanOptions options,
        IReadOnlySet<string> ignoredFolders,
        Action<LogEntry> log)
    {
        var pending = new Stack<string>();
        pending.Push(sourceFolder);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(current);
            }
            catch (Exception ex)
            {
                log(new LogEntry(LogLevel.Error, $"{current}: {ex.Message}"));
                continue;
            }

            foreach (var directory in directories)
            {
                var relativeDirectory = Path.GetRelativePath(sourceFolder, directory);
                if (IsIgnoredPath(relativeDirectory, options.UseCommonIgnores, ignoredFolders))
                {
                    log(new LogEntry(LogLevel.Skip, $"{relativeDirectory}{Path.DirectorySeparatorChar}"));
                    continue;
                }

                try
                {
                    var attributes = File.GetAttributes(directory);
                    if (!options.IncludeHiddenFiles && attributes.HasFlag(FileAttributes.Hidden))
                    {
                        log(new LogEntry(LogLevel.Skip, $"{relativeDirectory}{Path.DirectorySeparatorChar}"));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    log(new LogEntry(LogLevel.Error, $"{relativeDirectory}: {ex.Message}"));
                    continue;
                }

                pending.Push(directory);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(current);
            }
            catch (Exception ex)
            {
                log(new LogEntry(LogLevel.Error, $"{current}: {ex.Message}"));
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }
        }
    }

    private static bool IsIgnoredPath(
        string relativePath,
        bool useCommonIgnores,
        IReadOnlySet<string> ignoredFolders)
    {
        return IsUserIgnored(relativePath, ignoredFolders) ||
            (useCommonIgnores && IsCommonIgnored(relativePath));
    }

    private static bool IsUserIgnored(string relativePath, IReadOnlySet<string> ignoredFolders)
    {
        if (ignoredFolders.Count == 0)
        {
            return false;
        }

        var normalized = NormalizeRelativePath(relativePath);
        foreach (var ignored in ignoredFolders)
        {
            if (string.Equals(normalized, ignored, StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(ignored + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCommonIgnored(string relativePath)
    {
        var normalized = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var parts = normalized.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        foreach (var ignored in CommonIgnoredFolders)
        {
            var ignoredParts = ignored.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (ContainsSequence(parts, ignoredParts))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlySet<string> NormalizeIgnoredFolders(IReadOnlyList<string> ignoredFolders)
    {
        return ignoredFolders
            .Where(folder => !string.IsNullOrWhiteSpace(folder))
            .Select(NormalizeRelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        return relativePath
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/')
            .Trim('/');
    }

    private static bool ContainsSequence(string[] parts, string[] ignoredParts)
    {
        if (ignoredParts.Length == 0 || ignoredParts.Length > parts.Length)
        {
            return false;
        }

        for (var start = 0; start <= parts.Length - ignoredParts.Length; start++)
        {
            var matched = true;
            for (var offset = 0; offset < ignoredParts.Length; offset++)
            {
                if (!string.Equals(parts[start + offset], ignoredParts[offset], StringComparison.OrdinalIgnoreCase))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<Regex> ParsePatterns(string input)
    {
        return input
            .Split([',', ';', '\r', '\n', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(WildcardToRegex)
            .ToList();
    }

    private static Regex WildcardToRegex(string wildcard)
    {
        var escaped = Regex.Escape(wildcard)
            .Replace("\\*", ".*", StringComparison.Ordinal)
            .Replace("\\?", ".", StringComparison.Ordinal);

        return new Regex($"^{escaped}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool MatchesAnyPattern(string relativePath, IReadOnlyList<Regex> patterns)
    {
        var fileName = Path.GetFileName(relativePath);
        var normalizedPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

        return patterns.Any(pattern =>
            pattern.IsMatch(fileName) ||
            pattern.IsMatch(normalizedPath));
    }
}
