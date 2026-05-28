namespace Darkbox.Core.Domain;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Categories can be stacked inside Categories
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = new List<Category>();
    public List<Shoot> Shoots { get; set; } = new List<Shoot>();
}