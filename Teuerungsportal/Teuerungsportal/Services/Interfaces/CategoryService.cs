namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface CategoryService
{
    public Task<Category?> GetCategory(string categoryName);
    
    public Task<ICollection<Category>> GetCategoriesWithChildren();
    
    public Task<ICollection<Category>> GetAllCategories();

    public Task<int> GetNumberOfProducts(Guid categoryId);
}