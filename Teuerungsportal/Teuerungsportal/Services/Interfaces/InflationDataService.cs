namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface InflationDataService
{
    public Task<ICollection<InflationData>> GetInflationDataForMonth();

    public Task<ICollection<FilteredCount>> GetInflationData(string? filter);
}