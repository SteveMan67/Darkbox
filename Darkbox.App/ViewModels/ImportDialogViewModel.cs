using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Interfaces;
using Darkbox.Core.Domain;
using System.Linq;
using System.Threading.Tasks;
using Darkbox.Views;

namespace Darkbox.ViewModels;

public partial class ImportDialogViewModel : ViewModelBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IImportService _importService;

    public ObservableCollection<FileSystemItemViewModel> Drives { get; } = new();
    
    [ObservableProperty] private FileSystemItemViewModel? _selectedFolder;

    [ObservableProperty]
    private ObservableCollection<PhotoPreviewViewModel> _photos = new();
    
    public ImportDialogViewModel(IFileSystemService fileSystemService, IImportService importService)
    {
        _fileSystemService = fileSystemService;
        _importService = importService;
        LoadRootDrives();
    }

    async partial void OnSelectedFolderChanged(FileSystemItemViewModel? value)
    {
        if (value == null) return;

        var loadedPhotos = await Task.Run(async () =>
        {
            var photosInFolder = _importService.GetRawPhotosInFolder(value.FullPath);
            IProgress<int> progress = new Progress<int>(percent => Console.WriteLine($"Progress: {percent}%"));

            await _importService.ReadCaptureTime(photosInFolder, progress);

            return photosInFolder
                .OrderBy(p => p.CaptureTime)
                .Select(p => new PhotoPreviewViewModel(p))
                .ToList();

        });
        
        Photos = new ObservableCollection<PhotoPreviewViewModel>(loadedPhotos);
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