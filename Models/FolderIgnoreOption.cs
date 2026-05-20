using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectUpdateCloner.Models;

public sealed class FolderIgnoreOption : INotifyPropertyChanged
{
    private bool isIgnored;

    public FolderIgnoreOption(string relativePath, bool isIgnored)
    {
        RelativePath = relativePath;
        DisplayName = relativePath;
        this.isIgnored = isIgnored;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string RelativePath { get; }

    public string DisplayName { get; }

    public bool IsIgnored
    {
        get => isIgnored;
        set
        {
            if (isIgnored == value)
            {
                return;
            }

            isIgnored = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIgnored)));
        }
    }
}
