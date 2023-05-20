namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiPriceService : PriceService
{
    private const string BaseUrl = "https://api.teuerungsportal.at";
    private HttpClient Client { get; set; }

    public ApiPriceService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<int> GetPriceChangesPages()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetPriceChanges(int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }
}