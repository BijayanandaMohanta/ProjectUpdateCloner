using System.IO;
using System.IO.Compression;
using ProjectUpdateCloner.Models;

namespace ProjectUpdateCloner.Services;

public sealed class UpdatePackageService
{
    public CopyResult CreatePackage(
        string sourceFolder,
        string outputRoot,
        DateTime selectedDate,
        IReadOnlyList<ChangedFile> files,
        bool preserveStructure,
        bool createZip,
        Action<LogEntry> log,
        Action<int, int, string, bool>? progress = null)
    {
        var destinationFolder = CreateDestinationFolder(sourceFolder, outputRoot, selectedDate, DateTime.Now);
        var copied = new List<ChangedFile>();
        var errorCount = 0;

        Directory.CreateDirectory(destinationFolder);
        log(new LogEntry(LogLevel.Info, $"Created {destinationFolder}"));

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            try
            {
                var destinationPath = preserveStructure
                    ? Path.Combine(destinationFolder, file.RelativePath)
                    : Path.Combine(destinationFolder, Path.GetFileName(file.FullPath));

                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(file.FullPath, destinationPath, overwrite: true);
                copied.Add(file);
                log(new LogEntry(LogLevel.Success, $"Copied {file.RelativePath}"));
                progress?.Invoke(index + 1, files.Count, file.RelativePath, true);
            }
            catch (Exception ex)
            {
                errorCount++;
                log(new LogEntry(LogLevel.Error, $"{file.RelativePath}: {ex.Message}"));
                progress?.Invoke(index + 1, files.Count, file.RelativePath, false);
            }
        }

        var reportPath = Path.Combine(destinationFolder, "update-log.txt");
        WriteReport(reportPath, sourceFolder, destinationFolder, selectedDate, copied, errorCount);
        log(new LogEntry(LogLevel.Info, $"Report written {reportPath}"));

        string? zipPath = null;
        if (createZip && copied.Count > 0)
        {
            zipPath = destinationFolder + ".zip";
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(destinationFolder, zipPath, CompressionLevel.Optimal, includeBaseDirectory: true);
            log(new LogEntry(LogLevel.Success, $"Zip created {zipPath}"));
        }

        return new CopyResult(destinationFolder, zipPath, copied.Count, errorCount);
    }

    private static string CreateDestinationFolder(string sourceFolder, string outputRoot, DateTime selectedDate, DateTime generatedAt)
    {
        var projectName = new DirectoryInfo(sourceFolder).Name;
        var baseName = $"{projectName}-updates-{selectedDate:yyyy-MM-dd}-{generatedAt:HH-mm}";
        var destination = Path.Combine(outputRoot, baseName);

        if (!Directory.Exists(destination))
        {
            return destination;
        }

        return Path.Combine(outputRoot, $"{baseName}-{generatedAt:ss}");
    }

    private static void WriteReport(
        string reportPath,
        string sourceFolder,
        string destinationFolder,
        DateTime selectedDate,
        IReadOnlyList<ChangedFile> copied,
        int errorCount)
    {
        var lines = new List<string>
        {
            "Project Update Package Report",
            "=============================",
            $"Project: {new DirectoryInfo(sourceFolder).Name}",
            $"Source: {sourceFolder}",
            $"Destination: {destinationFolder}",
            $"Date filter: {selectedDate:yyyy-MM-dd}",
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"Copied files: {copied.Count}",
            $"Errors: {errorCount}",
            string.Empty,
            "Files",
            "-----"
        };

        lines.AddRange(copied.Select(file => file.RelativePath));
        File.WriteAllLines(reportPath, lines);
    }
}
