namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface StoreService
{
    public Task<ICollection<Store>> GetStores();
}