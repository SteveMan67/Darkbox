using System.Diagnostics;
using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;
using ImageMagick;
using ImageMagick.Formats;
using ImageMagick.Configuration;

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
        ".RWL",
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

        try
        {
            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

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
            }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return photos;
        }
        

        return photos;
    }

    public async Task ReadCaptureTime(List<Photo> photos, IProgress<int> progress)
    {
        await Task.Run(() =>
        {
            int processedCount = 0;

            Parallel.ForEach(photos, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                photo =>
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
                        
                        var ifd0Directory =  directories.OfType<ExifIfd0Directory>().FirstOrDefault();

                        if (ifd0Directory != null)
                        {
                            if (ifd0Directory.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                            {
                                photo.Orientation = orientation;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        photo.CaptureTime = File.GetCreationTime(photo.FilePath);
                    }

                    processedCount++;

                    var percentage = (int)((double)processedCount / photos.Count * 100);
                    progress?.Report(percentage);
                });

        });
    }

    public byte[]? GetEmbeddedPreviewBytes(string filePath)
    {
        ResourceLimits.Thread = 1;

        OpenCL.IsEnabled = false;
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            foreach (var directory in directories)
            {
                long offset = GetTagValueAsLong(directory, 0x0201);
                long length = GetTagValueAsLong(directory, 0x0202);

                if (offset == 0 || length == 0)
                {
                    offset = GetTagArrayFirstValueAsLong(directory, 0x0111);
                    length = GetTagArrayFirstValueAsLong(directory, 0x0117);
                }

                if (offset > 0 && length > 0)
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    stream.Seek(offset, SeekOrigin.Begin);

                    var buffer = new byte[length];
                    stream.Read(buffer, 0, (int)length);

                    if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xD8)
                    {
                        Debug.WriteLine($"Found embedded JPEG for {filePath}");
                        return buffer;
                    }
                }
            }

            Debug.WriteLine($"Falling back to ImageMagick for {filePath}");
            
            var settings = new MagickReadSettings(new DngReadDefines() { UseCameraWhiteBalance = true })
            {
                Width = 300, Height = 300
            };
            
            using var image = new MagickImage(filePath, settings);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 75;
            
            if (image.Width > 300 || image.Height > 300)
                    image.Resize(new MagickGeometry(300, 300) { IgnoreAspectRatio = false });

            var result =  image.ToByteArray();
            
            return result;
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to extract thumbnail for {filePath}: {ex.Message}");
            return null;
        }
    }

    public Task GetPhotoSizes(List<Photo> photos)
    {
        foreach (var photo in photos)
        {
            photo.FileSizeInBytes = new FileInfo(photo.FilePath).Length;
        }
        
        return Task.CompletedTask;
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
            photo.FileSizeInBytes = new FileInfo(photo.FilePath).Length;
            var directories = ImageMetadataReader.ReadMetadata(photo.FilePath);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfdDirectory != null)
            {
                photo.Lens = subIfdDirectory.GetString(ExifDirectoryBase.TagLensModel) ?? String.Empty;
                
                if (string.IsNullOrEmpty(photo.Lens))
                {
                    var lensSpec = subIfdDirectory.GetString(ExifDirectoryBase.TagLensSpecification);
                    if (!string.IsNullOrEmpty(lensSpec))
                        photo.Lens = lensSpec;
                }

                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focalLength))
                    photo.FocalLength = focalLength;
                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagFNumber, out var aperture))
                    photo.Aperture = aperture;
                if (subIfdDirectory.TryGetDouble(ExifDirectoryBase.TagExposureTime, out var exposureTime))
                    photo.ShutterSpeed = exposureTime;
                if (subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                    photo.Iso = iso;
            }

            var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0Directory != null)
            {
                var make = ifd0Directory.GetString(ExifDirectoryBase.TagMake)?.Trim() ?? "";
                var model = ifd0Directory.GetString(ExifDirectoryBase.TagModel)?.Trim() ?? "";
                photo.Camera = string.IsNullOrWhiteSpace(make) ? model : $"{make} {model}".Trim();
                
                
                if (ifd0Directory.TryGetInt32(ExifDirectoryBase.TagImageWidth, out var width))
                    photo.Width = width;
                if (ifd0Directory.TryGetInt32(ExifDirectoryBase.TagImageHeight, out var height))
                    photo.Height = height;
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

    private long GetTagValueAsLong(MetadataExtractor.Directory dir, int tagId)
    {
        if (!dir.ContainsTag(tagId)) return 0;
        try
        {
            return Convert.ToInt64(dir.GetObject(tagId));
        }
        catch
        {
            return 0;
        }
    }

    private long GetTagArrayFirstValueAsLong(MetadataExtractor.Directory dir, int tagId)
    {
        if (!dir.ContainsTag(tagId)) return 0;
        try
        {
            var obj = dir.GetObject(tagId);
            if (obj is int[] intArray && intArray.Length > 0) return intArray[0];
            if (obj is long[] longArray && longArray.Length > 0) return longArray[0];
            if (obj is uint[] uintArray && uintArray.Length > 0) return uintArray[0];
            return Convert.ToInt64(obj);
        }
        catch
        {
            return 0;
        }
    }
}