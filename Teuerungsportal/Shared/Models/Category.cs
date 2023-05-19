namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public Category? ParentCategory { get; set; }

    public Category()
    {
        this.Name = string.Empty;
    }

    public Category(CategoryDbo dbo)
    {
        this.Id = dbo.Id;
        this.Name = dbo.Name;

        this.ParentCategory = dbo.ParentCategoryId == null
                              ? null
                              : new Category()
                                {
                                    Id = (Guid)dbo.ParentCategoryId,
                                    Name = dbo.ParentCategoryName,
                                };
    }

    public static void ComputeChildCategories(List<CategoryDbo> data, ICollection<Category> categories, int recursionLevel)
    {
        while (true)
        {
            if (data.Count == 0)
            {
                return;
            }

            var currentData = data.First();

            var currentRecursionLevel = currentData.RecursionId.Split('.').Length;
            if (currentRecursionLevel < recursionLevel)
            {
                return;
            }

            data.RemoveAt(0);
            var category = new Category()
                           {
                               Id = currentData.Id,
                               Name = currentData.Name,
                               SubCategories = new List<Category>(),
                           };

            ComputeChildCategories(data, category.SubCategories, recursionLevel + 1);
            category.SubCategories = category.SubCategories.OrderBy(c => c.Name).ToList();
            categories.Add(category);
        }
    }
}