namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface CategoryService
{
    public Task<ICollection<Category>> GetCategoriesWithChildren();
    
    public Task<ICollection<Category>> GetAllCategories();
}