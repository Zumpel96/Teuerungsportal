namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface ProductService
{
    public Task<int> GetProductsWithoutCategoryPages();
    
    public Task<ICollection<Price>> GetProductsWithoutCategory(int page);

    public Task<int> GetProductsWithoutCategorySearchPages(string searchString);

    public Task<ICollection<Price>> GetProductsWithoutCategorySearch(string searchString, int page);
    
    public Task<Product?> GetProduct(string store, string productNumber);

    public Task<int> GetProductPriceChangesPages(Guid productId);

    public Task<ICollection<Price>> GetProductPriceChanges(Guid productId, int page);

    public Task<int> GetNewProductsPages();

    public Task<ICollection<Price>> GetNewProducts(int page, string filter, ICollection<string> storeNames);

    public Task<int> GetProductSearchPages(string searchString);

    public Task<ICollection<Price>> GetProductsSearch(string searchString, int page);
    
    public Task UpdateProductCategory(Guid productId, Guid categoryId);
    
    public Task<ICollection<FilteredCount>> GetAllProductCounts(string? filter);
}