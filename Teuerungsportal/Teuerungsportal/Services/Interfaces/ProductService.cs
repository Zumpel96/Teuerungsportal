namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface ProductService
{
    public Task<int> GetProductsWithoutCategoryPages();
    
    public Task<ICollection<Product>> GetProductsWithoutCategory(int page);
    
    public Task<Product?> GetProduct(string store, string productNumber);
    
    public Task UpdateProductCategory(Guid productId, Guid categoryId);
}