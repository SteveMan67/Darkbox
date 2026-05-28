namespace Darkbox.Core.Domain;
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Photo> Photos { get; set; } = new List<Photo>();
}