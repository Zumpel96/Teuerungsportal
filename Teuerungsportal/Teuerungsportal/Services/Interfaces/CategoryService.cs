namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface CategoryService
{
    public Task<Category?> GetCategory(string categoryName);
    
    public Task<ICollection<Category>> GetCategories();
    
    public Task<ICollection<Category>> GetUngroupedCategories();

    public Task<int> GetCategoryProductPages(Guid categoryId);
    
    public Task<ICollection<Product>> GetCategoryProducts(Guid categoryId, int page);

    public Task<int> GetCategoryPriceChangesPages(Guid categoryId);

    public Task<ICollection<Price>> GetCategoryPriceChanges(Guid categoryId, int page);
    
    public Task<ICollection<InflationData>> GetCategoryInflationData(Guid categoryId);
}