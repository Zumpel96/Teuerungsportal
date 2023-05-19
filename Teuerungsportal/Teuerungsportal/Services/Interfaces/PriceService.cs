namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface PriceService
{
    public Task<int> GetPriceChangesPages();
    
    public Task<ICollection<Price>> GetPriceChanges(int page);
}