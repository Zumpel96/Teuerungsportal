namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Helpers;

public interface PriceService
{
    public Task<ICollection<Price>> GetPriceChangesForProduct(Guid productId);
}