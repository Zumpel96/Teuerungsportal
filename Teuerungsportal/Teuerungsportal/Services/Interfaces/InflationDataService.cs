namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface InflationDataService
{
    public Task<ICollection<InflationData>> GetInflationDataForMonth();
    
    public Task<ICollection<InflationData>> GetInflationDataForStore(Guid storeId);

    public Task<ICollection<FilteredCount>> GetInflationData(string? filter);
    
    public Task<FilteredCount> GetStoreInflationData(string? filter, Guid storeId);
}