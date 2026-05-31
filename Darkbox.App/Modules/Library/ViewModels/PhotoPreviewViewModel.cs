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

namespace Darkbox.ViewModels;

public partial class PhotoPreviewViewModel : ViewModelBase
{
    public Photo Photo { get; }
    
    [ObservableProperty]
    private Bitmap? _previewImage;

    private bool _isImageLoading;

    public event Action<PhotoPreviewViewModel>? Selected;

    [RelayCommand]
    private void Select()
    {
        Selected?.Invoke(this);
    }
    
    private static readonly ConcurrentQueue<(PhotoPreviewViewModel Vm, IImportService ImportService)> _loadQueue = new();
    
    private static int _isWorkerRunning = 0;
    
    public PhotoPreviewViewModel(Photo photo)
    {
        Photo = photo;
    }

    public async Task LoadPreviewAsync(IImportService importService)
    {
        if (_previewImage != null || _isImageLoading)
            return;
                
        _isImageLoading = true;
        
        _loadQueue.Enqueue((this, importService));

        if (Interlocked.Exchange(ref _isWorkerRunning, 1) == 0)
        {
            Task.Run(ProcessQueue);
        }
    }

    private static void ProcessQueue()
    {
        var tasks = new List<Task>();

        while (_loadQueue.TryDequeue(out var request))
        {
            var captured = request;
            tasks.Add(Task.Run(() => ProcessSingle(captured.Vm, captured.ImportService)));

            if (tasks.Count >= 4)
            {
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            Task.WaitAll(tasks.ToArray());
        }
        
        Interlocked.Exchange(ref _isWorkerRunning, 0);
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
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    vm.PreviewImage = scaled;
                    vm._isImageLoading = false;
                    Debug.WriteLine($"UI update: {sw.ElapsedMilliseconds}ms");

                });
            }
            else
            {
                vm._isImageLoading = false;
            }
        }
        catch
        {
            vm._isImageLoading = false;
        }
        
    }

}