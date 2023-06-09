namespace Teuerungsportal.Services.Interfaces;

using global::Shared.DatabaseObjects;
using Teuerungsportal.Models;

public interface StoreService
{
    public Task<ICollection<Store>> GetStores();
    
    public Task<int> GetAllStoresCount();
    
    public Task<Store?> GetStore(string name);

    public Task<int> GetStoreProductsPages(Guid storeId);
    
    public Task<int> GetStorePriceChangesPages(Guid storeId);

    public Task<ICollection<Product>> GetStoreProducts(Guid storeId, int page);
    
    public Task<ICollection<Price>> GetStorePriceChanges(Guid storeId, int page);

    public Task<ICollection<InflationData>> GetStoreInflationData(Guid storeId);
}