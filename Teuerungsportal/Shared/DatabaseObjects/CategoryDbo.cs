namespace Shared.DatabaseObjects;

public class CategoryDbo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public Guid? ParentCategoryId { get; set; }

    public string ParentCategoryName { get; set; } = string.Empty;
    public string RecursionId { get; set; } = string.Empty;
}