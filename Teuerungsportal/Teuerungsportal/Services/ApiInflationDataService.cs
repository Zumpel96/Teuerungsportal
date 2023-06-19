namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiInflationDataService : InflationDataService
{
    private const string BaseUrl = "https://api.teuerungsportal.at";
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
    public async Task<ICollection<InflationData>> GetInflationDataForYear()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/chart/year");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<InflationData>>(responseBody);

        return data ?? new List<InflationData>();
    }
}