namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Helpers;
using Teuerungsportal.Services.Interfaces;

public class ApiPriceService : PriceService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net/api";
    private HttpClient Client { get; set; }

    public ApiPriceService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetPriceChangesForProduct(Guid productId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/product/{productId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetPriceChangesForCategory(Guid categoryId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/prices/category/{categoryId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }
}