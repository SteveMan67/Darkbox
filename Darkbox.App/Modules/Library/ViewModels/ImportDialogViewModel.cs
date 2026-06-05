using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Darkbox.Models;
using Darkbox.Modules.Library.ViewModels;
using Darkbox.ViewModels;


namespace Darkbox.Modules.Library.ViewModels;

public partial class ImportDialogViewModel : ViewModelBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IImportService _importService;

    [ObservableProperty]
    private ObservableCollection<PhotoShootViewModel>? _photoShoots;

    public ObservableCollection<FileSystemItemViewModel> Drives { get; } = new();
    
    [ObservableProperty] private FileSystemItemViewModel? _selectedFolder;

    public ImportDialogViewModel(IFileSystemService fileSystemService, IImportService importService)
    {
        _fileSystemService = fileSystemService;
        _importService = importService;
        LoadRootDrives();
    }

    [RelayCommand]
    private void Import()
    {
        
    }
    
    public event Action? ImportDialogCloseRequested;

    [RelayCommand]
    private void CloseImportDialog()
    {
        ImportDialogCloseRequested.Invoke();
    }
    
    [ObservableProperty]
    private PhotoPreviewViewModel? _selectedPhoto;

    [ObservableProperty] private int _columnCount = 5;

    private async void OnPhotoSelected(PhotoPreviewViewModel photo)
    {
        SelectedPhoto = null;
        await _importService.ReadFullExif(photo.Photo);
        SelectedPhoto = photo;
        UpdateMetadata(photo);
    }
    
    

    async partial void OnSelectedFolderChanged(FileSystemItemViewModel? value)
    {
        if (value == null) return;

        var photosInFolder = await Task.Run(async () =>
        {
            var photosInFolder = _importService.GetRawPhotosInFolder(value.FullPath);
            IProgress<int> progress = new Progress<int>(percent => Console.WriteLine($"Progress: {percent}%"));

            await _importService.ReadCaptureTime(photosInFolder, progress);
            await _importService.GetPhotoSizes(photosInFolder);

            return photosInFolder;
        });
        
        var shoots = _importService.GroupByShoot(photosInFolder)
            .Select(s =>
            {
                var vm = new PhotoShootViewModel(s);
                vm.PhotoSelected += OnPhotoSelected;
                return vm;
            })
            .ToList();
            
        PhotoShoots = new ObservableCollection<PhotoShootViewModel>(shoots);
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

    [ObservableProperty] private ObservableCollection<MetadataItem> _metadata = new();
    
    [ObservableProperty]
    private string _aperture = string.Empty;

    [ObservableProperty]
    private string _focalLength = string.Empty;

    [ObservableProperty]
    private string _iso = string.Empty;

    [ObservableProperty]
    private string _shutterSpeed = string.Empty;

    private void UpdateMetadata(PhotoPreviewViewModel photo)
    {
        var p = photo.Photo;
        Metadata = new ObservableCollection<MetadataItem>
        {
            new MetadataItem { Label = "Dimensions", Value = $"{p.Width}x{p.Height}" },
            new MetadataItem { Label = "Camera", Value = p.Camera },
            new MetadataItem { Label = "Lens", Value = p.Lens },
            new MetadataItem { Label = "Date Taken", Value = p.CaptureTime.ToString("M/d/yyyy") },
            new MetadataItem { Label = "File Size", Value = FormatFileSize(p.FileSizeInBytes) },
            new MetadataItem { Label = "Location", Value = p.FilePath },
        };
        Aperture = $"f{Math.Round(p.Aperture, 1)}";
        FocalLength = $"{p.FocalLength}mm";
        Iso = $"ISO {p.Iso}";
        ShutterSpeed = FormatShutterSpeed(p.ShutterSpeed);
    }
    
    private static string FormatShutterSpeed(double seconds)
    {
        if (seconds >= 1)
            return $"{seconds}s";
        return $"1/{(int)Math.Round(1.0 / seconds)}";
    }

    private static string FormatFileSize(long bytes)
    {
        return $"{bytes / 1_000_000.0:F1}MB";
    }
}