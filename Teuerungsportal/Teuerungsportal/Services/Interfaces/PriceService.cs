namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface PriceService
{
    public Task<ICollection<FilteredCount>> GetAllPriceChanges(string? filter);

    public Task<ICollection<Price>> GetTodayPriceChanges(int page, string filter, IEnumerable<string> storeNames);
    
    public Task<ICollection<Price>> GetTopPriceChanges();
    
    public Task<ICollection<Price>> GetWorstPriceChanges();

    public Task<ICollection<Price>> GetProductPriceChanges(Guid productId);
}