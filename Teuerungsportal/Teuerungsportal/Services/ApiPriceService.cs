namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiPriceService : PriceService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net";
    private HttpClient Client { get; set; }

    public ApiPriceService(HttpClient client)
    {
        this.Client = client;
    }
    
    /// <inheritdoc />
    public async Task<ICollection<Price>> GetPriceChanges()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
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
}