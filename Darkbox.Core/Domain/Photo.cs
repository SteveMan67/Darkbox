namespace Darkbox.Core.Domain;

public class Photo
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime CaptureTime { get; set; }
    public long FileSizeInBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Orientation { get; set; }
    public int ShootId  { get; set; }
    
    // Camera Metadata
    public string Camera { get; set; } = string.Empty;
    public string Lens { get; set; } = string.Empty;
    public double FocalLength { get; set; }
    public double Aperture { get; set; }
    public double ShutterSpeed { get; set; }
    public int Iso { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Catalog Data
    public bool IsMissing { get; set; }
    public string DriveSerial { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public int Rating  { get; set; }
    public bool IsFlagged { get; set; }
    public List<Tag> Tags { get; set; } = new List<Tag>();
}
