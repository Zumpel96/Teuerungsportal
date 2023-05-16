namespace Teuerungsportal.Helpers;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string RecursionId { get; set; } = string.Empty;

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public ICollection<Category> ParentCategories { get; set; } = new List<Category>();
}