namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface PriceService
{
    public Task<ICollection<Price>> GetPriceChanges();
}