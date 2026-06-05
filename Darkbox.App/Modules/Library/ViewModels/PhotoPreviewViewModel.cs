using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Darkbox.ViewModels;
using ImageMagick;

namespace Darkbox.Modules.Library.ViewModels;

public partial class PhotoPreviewViewModel : ViewModelBase
{
    public Photo Photo { get; }
    
    [ObservableProperty]
    private Bitmap? _previewImage;

    [ObservableProperty] private bool _isSelected;

    private int _isImageLoading = 0;

    public event Action<PhotoPreviewViewModel>? Selected;

    [RelayCommand]
    private void Select()
    {
        if (IsSelected == true)
        {
            IsSelected = false;
        }
        else
        {
            IsSelected = true;
        }
    }
    
    private static readonly ConcurrentQueue<(PhotoPreviewViewModel Vm, IImportService ImportService)> _loadQueue = new();
    
    private static int _isWorkerRunning = 0;
    
    public PhotoPreviewViewModel(Photo photo)
    {
        Photo = photo;
    }

    public async Task LoadPreviewAsync(IImportService importService)
    {
        if (PreviewImage != null || Interlocked.Exchange(ref _isImageLoading, 1) == 1)
            return;
                
        _loadQueue.Enqueue((this, importService));

        if (Interlocked.Exchange(ref _isWorkerRunning, 1) == 0)
        {
            Task.Run(ProcessQueue);
        }
    }

    private static async Task ProcessQueue()
    {
        var tasks = new List<Task>();

        while (_loadQueue.TryDequeue(out var request))
        {
            var captured = request;
            Task.Run(() => ProcessSingle(captured.Vm, captured.ImportService));
        }

        await Task.Delay(100);
        
        Interlocked.Exchange(ref _isWorkerRunning, 0);
        
        if (!_loadQueue.IsEmpty && Interlocked.Exchange(ref _isWorkerRunning, 1) == 0)
            Task.Run(ProcessQueue);
    }

    private static void ProcessSingle(PhotoPreviewViewModel vm, IImportService importService)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            byte[]? jpegBytes = importService.GetEmbeddedPreviewBytes(vm.Photo.FilePath);
            Debug.WriteLine($"GetEmbeddedPreviewBytes: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            
            if (jpegBytes != null && jpegBytes.Length > 0)
            {
                using var inputStream = new MemoryStream(jpegBytes);
                using var fullBitmap = new Bitmap(inputStream);

                var maxSize = 300;
                var scale = Math.Min((double)maxSize / fullBitmap.PixelSize.Width, (double)maxSize / fullBitmap.PixelSize.Height);
                var newHeight = (int)(fullBitmap.PixelSize.Height * scale);
                var newWidth = (int)(fullBitmap.PixelSize.Width * scale);
                
                var scaled = fullBitmap.CreateScaledBitmap(new Avalonia.PixelSize(newWidth, newHeight), BitmapInterpolationMode.LowQuality);
                
                Debug.WriteLine($"Bitmap decode: {sw.ElapsedMilliseconds}ms");
                sw.Restart();
                
                var rotated = applyOrientation(scaled, vm.Photo.Orientation);
                Debug.WriteLine($"File: {Path.GetFileName(vm.Photo.FilePath)}, Orientation: {vm.Photo.Orientation}");
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    vm.PreviewImage = rotated;
                    Interlocked.Exchange(ref vm._isImageLoading, 0);
                    Debug.WriteLine($"UI update: {sw.ElapsedMilliseconds}ms");

                });
            }
            else
            {
                vm._isImageLoading = 0;
            }
        }
        catch
        {
            vm._isImageLoading = 0;
        }
        
    }

    private static Bitmap applyOrientation(Bitmap bitmap, int orientation)
    {
        Debug.WriteLine($"ApplyOrientation called with: {orientation}");
        return orientation switch
        {
            3 => RotateBitmap(bitmap, 180),
            6 => RotateBitmap(bitmap, 90),
            8 => RotateBitmap(bitmap, 270),
            _ => bitmap  // 1 or anything else = no rotation
        };
    }

    private static Bitmap RotateBitmap(Bitmap bitmap, int degrees)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Position = 0;
    
        using var image = new MagickImage(stream);
        image.Rotate(degrees);
    
        using var outStream = new MemoryStream();
        image.Write(outStream, MagickFormat.Png);
        outStream.Position = 0;
    
        return new Bitmap(outStream);
    }
}