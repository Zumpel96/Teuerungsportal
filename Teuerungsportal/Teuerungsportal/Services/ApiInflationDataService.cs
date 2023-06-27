namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiInflationDataService : InflationDataService
{
    private const string BaseUrl = "https://api.teuerungsportal.at/v2";
    private HttpClient Client { get; set; }

    public ApiInflationDataService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<ICollection<InflationData>> GetInflationDataForMonth()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/chart");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<InflationData>>(responseBody);
        
        return data ?? new List<InflationData>();
    }

    /// <inheritdoc />
    public async Task<ICollection<InflationData>> GetInflationDataForStore(Guid storeId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/chart/{storeId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<ICollection<InflationData>>(responseBody);

        return data ?? new List<InflationData>();
    }

    /// <inheritdoc />
    public async Task<ICollection<FilteredCount>> GetInflationData(string? filter)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/inflation/total{filter}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<FilteredCount>>(responseBody);

        return data ?? new List<FilteredCount>();
    }

    /// <inheritdoc />
    public async Task<FilteredCount> GetStoreInflationData(string? filter, Guid storeId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/inflation/store/{storeId}{filter}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<FilteredCount>(responseBody);

        return data ?? new FilteredCount();
    }
}