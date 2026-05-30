using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Interfaces;

namespace Darkbox.ViewModels;

public partial class ImportDialogViewModel : ViewModelBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IImportService _importService;

    public ObservableCollection<FileSystemItemViewModel> Drives { get; } = new();
    
    [ObservableProperty] private FileSystemItemViewModel _selectedFolder;
    
    public ImportDialogViewModel(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        LoadRootDrives();
    }

    partial void OnSelectedFolderChanged(FileSystemItemViewModel? value)
    {
        if (value is null || value.Name == "Loading...")
            return;

        string selectedPath = value.FullPath;
        
        Console.WriteLine($"Selected path: {selectedPath}");
        var PhotosInFolder = _importService.GetRawPhotosInFolder(selectedPath);
        
    }

    private void LoadRootDrives()
    {
        Drives.Clear();
        var drives = _fileSystemService.GetDrives();

        foreach (var drive in drives)
        {
            Drives.Add(new FileSystemItemViewModel(drive, _fileSystemService));
        }
    }
}