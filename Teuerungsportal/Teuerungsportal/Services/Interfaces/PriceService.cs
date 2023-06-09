namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface PriceService
{
    public Task<ICollection<Price>> GetPriceChanges();
    
    public Task<ICollection<Price>> GetTodayPriceChanges();
    
    public Task<ICollection<Price>> GetTopPriceChanges();
    
    public Task<ICollection<Price>> GetWorstPriceChanges();
}