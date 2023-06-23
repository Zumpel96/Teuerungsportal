namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiPriceService : PriceService
{
    private const string BaseUrl = "https://api.teuerungsportal.at/v2";
    private HttpClient Client { get; set; }

    public ApiPriceService(HttpClient client)
    {
        this.Client = client;
    }
    
    /// <inheritdoc />
    public async Task<ICollection<FilteredCount>> GetAllPriceChanges(string? filter)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/total{filter}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<FilteredCount>>(responseBody);

        return data ?? new List<FilteredCount>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetTodayPriceChanges()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/today");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetTopPriceChanges()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/top");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetWorstPriceChanges()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/worst");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetProductPriceChanges(Guid productId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/product/{productId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }
}