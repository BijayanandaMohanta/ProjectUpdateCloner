using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ProjectUpdateCloner.Models;
using ProjectUpdateCloner.Services;
using Forms = System.Windows.Forms;
using Wpf = System.Windows;

namespace ProjectUpdateCloner.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FileScannerService scanner;
    private readonly UpdatePackageService packageService;
    private readonly StringBuilder logBuilder = new();

    private string sourceFolder = string.Empty;
    private string outputFolder = string.Empty;
    private DateTime? selectedDate = DateTime.Today;
    private bool preserveStructure = true;
    private bool createZip = true;
    private bool useCommonIgnores = true;
    private bool includeHiddenFiles;
    private string filePatterns = string.Empty;
    private string logText = "Ready.";
    private string statusText = "Select A Project Folder And Date.";
    private string progressText = "Idle";
    private double progressValue;
    private bool isProgressIndeterminate;
    private int copiedCount;
    private int skippedCount;
    private int matchedCount;
    private int errorCount;
    private bool isBusy;
    private string folderListStatus = "Select A Project Folder To Load Folders.";
    private string? lastDestinationFolder;
    private bool isDestinationAvailable;

    public MainViewModel(FileScannerService scanner, UpdatePackageService packageService)
    {
        this.scanner = scanner;
        this.packageService = packageService;

        BrowseSourceCommand = new RelayCommand(() => BrowseForFolder(path => SourceFolder = path));
        BrowseOutputCommand = new RelayCommand(() => BrowseForFolder(path => OutputFolder = path));
        GenerateCommand = new AsyncRelayCommand(GenerateAsync, () => !IsBusy);
        OpenDestinationCommand = new RelayCommand(OpenDestinationFolder, () => IsDestinationAvailable);
        RefreshCommand = new RelayCommand(RefreshForNewWork, () => !IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RelayCommand BrowseSourceCommand { get; }

    public RelayCommand BrowseOutputCommand { get; }

    public AsyncRelayCommand GenerateCommand { get; }

    public RelayCommand OpenDestinationCommand { get; }

    public RelayCommand RefreshCommand { get; }

    public ObservableCollection<FolderIgnoreOption> ProjectFolders { get; } = [];

    public string SourceFolder
    {
        get => sourceFolder;
        set
        {
            if (SetField(ref sourceFolder, value) &&
                string.IsNullOrWhiteSpace(OutputFolder) &&
                Directory.Exists(value))
            {
                var parent = Directory.GetParent(value);
                if (parent is not null)
                {
                    OutputFolder = parent.FullName;
                }
            }

            if (Directory.Exists(value))
            {
                _ = LoadProjectFoldersAsync(value);
            }
            else
            {
                ProjectFolders.Clear();
                FolderListStatus = "Select A Project Folder To Load Folders.";
            }
        }
    }

    public string OutputFolder
    {
        get => outputFolder;
        set => SetField(ref outputFolder, value);
    }

    public DateTime? SelectedDate
    {
        get => selectedDate;
        set => SetField(ref selectedDate, value);
    }

    public bool PreserveStructure
    {
        get => preserveStructure;
        set => SetField(ref preserveStructure, value);
    }

    public bool CreateZip
    {
        get => createZip;
        set => SetField(ref createZip, value);
    }

    public bool UseCommonIgnores
    {
        get => useCommonIgnores;
        set => SetField(ref useCommonIgnores, value);
    }

    public bool IncludeHiddenFiles
    {
        get => includeHiddenFiles;
        set => SetField(ref includeHiddenFiles, value);
    }

    public string FilePatterns
    {
        get => filePatterns;
        set => SetField(ref filePatterns, value);
    }

    public string LogText
    {
        get => logText;
        private set => SetField(ref logText, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ProgressText
    {
        get => progressText;
        private set => SetField(ref progressText, value);
    }

    public double ProgressValue
    {
        get => progressValue;
        private set => SetField(ref progressValue, value);
    }

    public bool IsProgressIndeterminate
    {
        get => isProgressIndeterminate;
        private set => SetField(ref isProgressIndeterminate, value);
    }

    public string FolderListStatus
    {
        get => folderListStatus;
        private set => SetField(ref folderListStatus, value);
    }

    public int CopiedCount
    {
        get => copiedCount;
        private set => SetField(ref copiedCount, value);
    }

    public int SkippedCount
    {
        get => skippedCount;
        private set => SetField(ref skippedCount, value);
    }

    public int MatchedCount
    {
        get => matchedCount;
        private set => SetField(ref matchedCount, value);
    }

    public int ErrorCount
    {
        get => errorCount;
        private set => SetField(ref errorCount, value);
    }

    public bool IsDestinationAvailable
    {
        get => isDestinationAvailable;
        private set
        {
            if (SetField(ref isDestinationAvailable, value))
            {
                OpenDestinationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private async Task GenerateAsync()
    {
        if (!ValidateInputs())
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        ResetRunState();
        IsDestinationAvailable = false;
        lastDestinationFolder = null;

        try
        {
            var options = new ScanOptions
            {
                SourceFolder = SourceFolder,
                SelectedDate = SelectedDate!.Value,
                UseCommonIgnores = UseCommonIgnores,
                IncludeHiddenFiles = IncludeHiddenFiles,
                FilePatterns = FilePatterns,
                IgnoredRelativeFolders = ProjectFolders
                    .Where(folder => folder.IsIgnored)
                    .Select(folder => folder.RelativePath)
                    .ToList()
            };

            StatusText = "Scanning Files...";
            ProgressText = "Scanning Project Files...";
            var files = await Task.Run(() => scanner.Scan(options, AppendLog));
            MatchedCount = files.Count;

            if (files.Count == 0)
            {
                AppendLog(new LogEntry(LogLevel.Info, $"No files modified on {SelectedDate!.Value:yyyy-MM-dd}."));
                StatusText = "No Matching Files Found.";
                ProgressValue = 0;
                ProgressText = "No Matching Files Found.";
                IsProgressIndeterminate = false;
                return;
            }

            StatusText = "Creating Update Package...";
            IsProgressIndeterminate = false;
            ProgressValue = 0;
            ProgressText = $"Copying 0 of {files.Count} files...";
            var result = await Task.Run(() => packageService.CreatePackage(
                SourceFolder,
                OutputFolder,
                SelectedDate!.Value,
                files,
                PreserveStructure,
                CreateZip,
                AppendLog,
                ReportCopyProgress));

            StatusText = $"Done. Copied {result.CopiedCount} File(s).";
            lastDestinationFolder = result.DestinationFolder;
            IsDestinationAvailable = Directory.Exists(result.DestinationFolder);
            CopiedCount = result.CopiedCount;
            ErrorCount = Math.Max(ErrorCount, result.ErrorCount);
            ProgressValue = 100;
            ProgressText = result.ZipPath is null
                ? $"Copied {result.CopiedCount} File(s)."
                : $"Copied {result.CopiedCount} File(s) And Created Zip.";
            if (result.ErrorCount > 0)
            {
                StatusText += $" {result.ErrorCount} Error(s).";
            }
        }
        catch (Exception ex)
        {
            AppendLog(new LogEntry(LogLevel.Error, ex.Message));
            StatusText = "Failed.";
            ProgressText = "Failed.";
            IsProgressIndeterminate = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(SourceFolder) || !Directory.Exists(SourceFolder))
        {
            Wpf.MessageBox.Show("Select A Valid Project Folder.", "Missing Project Folder", Wpf.MessageBoxButton.OK, Wpf.MessageBoxImage.Warning);
            return false;
        }

        if (SelectedDate is null)
        {
            Wpf.MessageBox.Show("Select A Valid Update Date.", "Missing Update Date", Wpf.MessageBoxButton.OK, Wpf.MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            var parent = Directory.GetParent(SourceFolder);
            OutputFolder = parent?.FullName ?? SourceFolder;
        }

        if (!Directory.Exists(OutputFolder))
        {
            Wpf.MessageBox.Show("Select A Valid Output Folder.", "Missing Output Folder", Wpf.MessageBoxButton.OK, Wpf.MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static void BrowseForFolder(Action<string> setPath)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            setPath(dialog.SelectedPath);
        }
    }

    private void ClearLog()
    {
        logBuilder.Clear();
        LogText = string.Empty;
    }

    private void ResetRunState()
    {
        ProgressValue = 0;
        ProgressText = "Starting...";
        IsProgressIndeterminate = true;
        CopiedCount = 0;
        SkippedCount = 0;
        MatchedCount = 0;
        ErrorCount = 0;
    }

    private void OpenDestinationFolder()
    {
        if (string.IsNullOrWhiteSpace(lastDestinationFolder) || !Directory.Exists(lastDestinationFolder))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = lastDestinationFolder,
            UseShellExecute = true
        });
    }

    private void RefreshForNewWork()
    {
        sourceFolder = string.Empty;
        outputFolder = string.Empty;
        filePatterns = string.Empty;
        selectedDate = DateTime.Today;
        lastDestinationFolder = null;

        ProjectFolders.Clear();
        ClearLog();
        LogText = "Ready.";
        StatusText = "Select A Project Folder And Date.";
        ProgressValue = 0;
        ProgressText = "Idle";
        IsProgressIndeterminate = false;
        CopiedCount = 0;
        SkippedCount = 0;
        MatchedCount = 0;
        ErrorCount = 0;
        IsDestinationAvailable = false;
        FolderListStatus = "Select A Project Folder To Load Folders.";

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceFolder)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputFolder)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePatterns)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDate)));
    }

    private void AppendLog(LogEntry entry)
    {
        var prefix = entry.Level switch
        {
            LogLevel.Success => "[OK]",
            LogLevel.Skip => "[SKIP]",
            LogLevel.Error => "[ERROR]",
            _ => "[INFO]"
        };

        logBuilder.AppendLine($"{DateTime.Now:HH:mm:ss} {prefix} {entry.Message}");
        Wpf.Application.Current.Dispatcher.Invoke(() =>
        {
            if (entry.Level == LogLevel.Skip)
            {
                SkippedCount++;
            }
            else if (entry.Level == LogLevel.Error)
            {
                ErrorCount++;
            }
            else if (entry.Level == LogLevel.Success && entry.Message.StartsWith("Matched ", StringComparison.OrdinalIgnoreCase))
            {
                MatchedCount++;
            }

            LogText = logBuilder.ToString();
        });
    }

    private void ReportCopyProgress(int completed, int total, string relativePath, bool copied)
    {
        Wpf.Application.Current.Dispatcher.Invoke(() =>
        {
            if (copied)
            {
                CopiedCount++;
            }

            ProgressValue = total == 0 ? 0 : completed * 100d / total;
            ProgressText = $"Copying {completed} of {total}: {relativePath}";
        });
    }

    private async Task LoadProjectFoldersAsync(string folder)
    {
        FolderListStatus = "Loading Folders...";

        try
        {
            var sourceFolder = Path.GetFullPath(folder);
            var folders = await Task.Run(() => FindProjectFolders(sourceFolder));

            Wpf.Application.Current.Dispatcher.Invoke(() =>
            {
                ProjectFolders.Clear();
                foreach (var item in folders)
                {
                    ProjectFolders.Add(item);
                }

                FolderListStatus = folders.Count == 0
                    ? "No Folders Found In Selected Project."
                    : $"{folders.Count} Folder(s) Found. Checked Folders Are Skipped Completely.";
            });
        }
        catch (Exception ex)
        {
            FolderListStatus = $"Could Not Load Folders: {ex.Message}";
        }
    }

    private static IReadOnlyList<FolderIgnoreOption> FindProjectFolders(string sourceFolder)
    {
        try
        {
            return Directory
                .EnumerateDirectories(sourceFolder)
                .Select(directory =>
                {
                    var relativePath = Path.GetRelativePath(sourceFolder, directory);
                    return new FolderIgnoreOption(relativePath, IsDefaultIgnoredFolder(relativePath));
                })
                .OrderBy(folder => folder.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static bool IsDefaultIgnoredFolder(string relativePath)
    {
        var parts = relativePath
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        return parts.Any(part =>
            part.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("vendor", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("node_modules", StringComparison.OrdinalIgnoreCase)) ||
            ContainsFolderSequence(parts, ["storage", "logs"]);
    }

    private static bool ContainsFolderSequence(string[] parts, string[] sequence)
    {
        if (sequence.Length > parts.Length)
        {
            return false;
        }

        for (var start = 0; start <= parts.Length - sequence.Length; start++)
        {
            var matched = true;
            for (var index = 0; index < sequence.Length; index++)
            {
                if (!parts[start + index].Equals(sequence[index], StringComparison.OrdinalIgnoreCase))
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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
