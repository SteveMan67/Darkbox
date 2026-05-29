namespace Darkbox.Core.Domain;

public class Shoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    
    // woah we reference the Photo class coolio
    public List<Photo> Photos { get; set; } = new List<Photo>();
}