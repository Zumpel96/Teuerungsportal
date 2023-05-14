namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface ProductService
{
    public Task<Product?> GetProduct(string store, string productNumber);
}