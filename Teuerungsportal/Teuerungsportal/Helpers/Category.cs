namespace Teuerungsportal.Helpers;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
}