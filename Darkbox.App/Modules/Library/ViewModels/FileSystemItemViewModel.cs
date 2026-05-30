using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Interfaces;

namespace Darkbox.ViewModels;

public partial class FileSystemItemViewModel : ViewModelBase
{
    private readonly IFileSystemService _fileSystemService;
    private bool _isLoaded;
    
    public string FullPath { get; }
    public string Name { get; }
    public ObservableCollection<FileSystemItemViewModel>? Subfolders { get; }

    [ObservableProperty] private bool _isExpanded;

    partial void OnIsExpandedChanged(bool value)
    {
        if (value && !_isLoaded)
        {
            LoadChildren();
        }
    }

    public FileSystemItemViewModel(string path, IFileSystemService fileSystemService, bool isDummy = false)
    {
        _fileSystemService = fileSystemService;
        FullPath = path;
        Name = isDummy ? "Loading..." : (Path.GetFileName(path) is { Length: > 0} name ? name : path);

        if (!isDummy)
        {
            Subfolders = new ObservableCollection<FileSystemItemViewModel>();

            if (_fileSystemService.HasSubDirectories(FullPath))
            {
                Subfolders.Add(new FileSystemItemViewModel(string.Empty, _fileSystemService, true));
            }
        }
    }

    private void LoadChildren()
    {
        _isLoaded = true;
        Subfolders!.Clear();
        
        var directories = _fileSystemService.GetDirectories(FullPath);
        foreach (var dir in directories)
        {
            Subfolders.Add(new FileSystemItemViewModel(dir, _fileSystemService));
        }
    }
}