using Darkbox.Core.Domain;

namespace Darkbox.Core.Interfaces;

public interface IImportService
{
    List<Photo> GetRawPhotosInFolder(string folderPath);
    Task ReadCaptureTime(List<Photo> photos, IProgress<int> progress);
    byte[]? GetEmbeddedPreviewBytes(string filePath);
    List<Shoot> GroupByShoot(List<Photo> photos);
    Task CopyFiles(List<Photo> photos, string destinationBasePath, IProgress<int> progress);
    Task ImportShoots(Dictionary<Shoot, List<Category>> shootsWithCategories);
    Task ReadFullExif(Photo photo);
}