using System.Diagnostics;
using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Util;
using Directory = System.IO.Directory;

namespace Darkbox.Core.Services;

public class ImportService : IImportService
{
    private readonly ICatalogRepository _catalog;
    
    public ImportService(ICatalogRepository catalog)
    {
        _catalog = catalog;
    }
    
    private static readonly HashSet<string> RawExtensions = new(StringComparer.OrdinalIgnoreCase) {
        // Canon
        ".CR2", ".CR3", ".CRW",
        // Nikon
        ".NEF", ".NRW",
        // Sony
        ".ARW", ".SRF", ".SR2",
        // Fujifilm
        ".RAF",
        // Olympus/OM System
        ".ORF",
        // Panasonic
        ".RW2",
        // Pentax
        ".PEF", ".DNG",
        // Leica
        ".DNG", ".RWL",
        // Samsung
        ".SRW",
        // Sigma
        ".X3F",
        // Adobe
        ".DNG",
        // Hasselblad
        ".3FR", ".FFF",
        // Phase One
        ".IIQ",
        // Also common formats
        ".TIFF", ".TIF"
    };

    public List<Photo> GetRawPhotosInFolder(string folderPath)
    {
        
        var photos =  new List<Photo>();

        var allFiles = System.IO.Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

        foreach (var rawFile in allFiles)
        {
            if (RawExtensions.Contains(Path.GetExtension(rawFile).ToUpper()))
            {
                photos.Add(new Photo
                {
                    FilePath = rawFile
                });
            }
        }

        return photos;
    }

    public async Task ReadCaptureTime(List<Photo> photos, IProgress<int> progress)
    {
        int processedCount = 0;

        foreach (var photo in photos)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(photo.FilePath);

                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                if (subIfdDirectory != null &&
                    subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var captureTime))
                {
                    photo.CaptureTime = captureTime;
                }
                else
                {
                    photo.CaptureTime = File.GetCreationTime(photo.FilePath);
                }
            }
            catch (Exception)
            {
                photo.CaptureTime = File.GetCreationTime(photo.FilePath);
            }

            processedCount++;

            var percentage = (int)((double)processedCount / photos.Count * 100);
            progress?.Report(percentage);
        }
    }

    public List<Shoot> GroupByShoot(List<Photo> photos)
    {
        if (photos == null || photos.Count == 0)
            return new List<Shoot>();

        var sortedPhotos = photos.OrderBy(p => p.CaptureTime).ToList();

        var shoots = new List<Shoot>();

        // initialize the first shoot with the first photo
        var currentShoot = new Shoot()
        {
            DateStart = sortedPhotos[0].CaptureTime,
            DateEnd = sortedPhotos[0].CaptureTime,
            Photos = new List<Photo> { sortedPhotos[0] }
        };

        shoots.Add(currentShoot);
        
        for (int i = 1; i < sortedPhotos.Count; i++)
        {
            var currentPhoto = sortedPhotos[i];
            var previousPhoto = sortedPhotos[i - 1];
            TimeSpan timeDifference = currentPhoto.CaptureTime - previousPhoto.CaptureTime;
            
            if (timeDifference.TotalHours >= 6.0)
            {
                currentShoot = new Shoot
                {
                    DateStart = currentPhoto.CaptureTime,
                    DateEnd = currentPhoto.CaptureTime,
                    Photos = new List<Photo> { currentPhoto }
                };
                shoots.Add(currentShoot);
            }
            else
            {
                // update the current shoot
                currentShoot.Photos.Add(currentPhoto);
                currentShoot.DateEnd = currentPhoto.CaptureTime;
            }
        }

        return shoots;
    }

    public async Task CopyFiles(List<Photo> photos, string destinationBasePath, IProgress<int> progress)
    {
        await Task.Run(() =>
        {
            int processedCount = 0;

            foreach (var photo in photos)
            {
                try
                {
                    var originalName = Path.GetFileName(photo.FilePath);
                    var captureDate = photo.CaptureTime != default
                        ? photo.CaptureTime
                        : File.GetCreationTime(photo.FilePath);

                    var yearFolder = captureDate.ToString("yyyy");
                    var monthFolder = captureDate.ToString("MM");
                    var targetDirectory = Path.Combine(destinationBasePath, yearFolder, monthFolder);

                    Directory.CreateDirectory(targetDirectory);

                    var datePrefix = captureDate.ToString("yyyy-MM-dd");
                    var newFileName = $"{datePrefix}_{originalName}";
                    var newFilePath = Path.Combine(targetDirectory, newFileName);

                    int counter = 1;
                    while (File.Exists(newFilePath))
                    {
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
                        var ext = Path.GetExtension(originalName);
                        newFileName = $"{datePrefix}_{nameWithoutExt}{ext}";
                        newFilePath = Path.Combine(targetDirectory, newFileName);
                        counter++;
                    }

                    File.Copy(photo.FilePath, newFilePath, overwrite: false);

                    photo.FilePath = newFilePath;
                    photo.ImportedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to copy {photo.FilePath}: {ex.Message}");
                }

                processedCount++;
                var percentage = (int)(double)processedCount / photos.Count * 100;
                progress?.Report(percentage);
            }
        });
    }

    public async Task ImportShoots(Dictionary<Shoot, List<Category>> shootsWithCategories)
    {
        foreach (var shootsWithCategory in shootsWithCategories)
        {
            var shoot = shootsWithCategory.Key;
            var categories = shootsWithCategory.Value;
            
            await _catalog.SaveShoot(shoot);
            foreach (var category in categories)
            {
                await _catalog.AddShootToCategory(shoot.Id, category);
            }

            foreach (var photo in shootsWithCategory.Key.Photos)
            {
                photo.ShootId = shoot.Id;
                await ReadFullExif(photo);
                await _catalog.SavePhoto(photo);
            }
        }
    }

    public async Task ReadFullExif(Photo photo)
    {
        try
        {

            var directories = ImageMetadataReader.ReadMetadata(photo.FilePath);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfdDirectory != null)
            {
                photo.Lens = subIfdDirectory.GetString(ExifDirectoryBase.TagLensModel) ?? String.Empty;

                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focalLength))
                    photo.FocalLength = focalLength;
                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagFNumber, out var aperture))
                    photo.Aperture = aperture;
                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagExposureTime, out var exposureTime))
                    photo.ShutterSpeed = exposureTime;
                if (subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                    photo.Iso = iso;
                if (subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagExifImageWidth, out var width))
                    photo.Width = width;
                if (subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagExifImageHeight, out var height))
                    photo.Height = height;
            }

            var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0Directory != null)
            {
                var make = ifd0Directory.GetString(ExifDirectoryBase.TagMake)?.Trim() ?? "";
                var model = ifd0Directory.GetString(ExifDirectoryBase.TagModel)?.Trim() ?? "";
                photo.Camera = string.IsNullOrWhiteSpace(make) ? model : $"{make} {model}".Trim();
            }

            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsDirectory != null)
            {
                if (gpsDirectory.TryGetGeoLocation(out var location))
                {
                    photo.Latitude = location.Latitude;
                    photo.Longitude = location.Longitude;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}