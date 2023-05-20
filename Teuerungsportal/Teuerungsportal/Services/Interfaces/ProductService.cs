namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface ProductService
{
    public Task<int> GetProductsWithoutCategoryPages();
    
    public Task<ICollection<Product>> GetProductsWithoutCategory(int page);
    
    public Task<Product?> GetProduct(string store, string productNumber);

    public Task<int> GetProductPriceChangesPages(Guid productId);

    public Task<ICollection<Price>> GetProductPriceChanges(Guid productId, int page);

    public Task<int> GetProductSearchPages(string searchString);

    public Task<ICollection<Product>> GetProductsSearch(string searchString, int page);
    
    public Task UpdateProductCategory(Guid productId, Guid categoryId);
}