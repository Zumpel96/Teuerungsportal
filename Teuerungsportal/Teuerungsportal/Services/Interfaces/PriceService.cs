namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface PriceService
{
    public Task<ICollection<FilteredCount>> GetAllPriceChanges(string? filter);
    
    public Task<ICollection<FilteredCount>> GetFavoritesPriceChangesCount(string? filter, ICollection<Guid> favorites);

    public Task<ICollection<Price>> GetFavoritesPriceChanges(string? filter, int page, ICollection<Guid> favorites);
    
    public Task<FilteredCount> GetStorePriceChangesCount(string? filter, Guid storeId);
    
    public Task<ICollection<Price>> GetStorePriceChanges(string? filter, int page, Guid storeId);

    public Task<ICollection<Price>> GetTodayPriceChanges(int page, string filter, IEnumerable<string> storeNames);
    
    public Task<ICollection<Price>> GetTopPriceChanges();
    
    public Task<ICollection<Price>> GetWorstPriceChanges();

    public Task<ICollection<Price>> GetProductPriceChanges(Guid productId);
}