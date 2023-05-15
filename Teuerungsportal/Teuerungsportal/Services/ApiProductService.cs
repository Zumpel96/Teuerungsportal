namespace Teuerungsportal.Services;

using System.Net;
using Newtonsoft.Json;
using Teuerungsportal.Helpers;
using Teuerungsportal.Services.Interfaces;

public class ApiProductService : ProductService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net/api";
    private HttpClient Client { get; set; }

    public ApiProductService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<Product?> GetProduct(string store, string productNumber)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{store}/{productNumber}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Product>(responseBody);

        return data ?? null;
    }

    /// <inheritdoc />
    public async Task UpdateProductCategory(Guid productId, Guid categoryId)
    {
        var response = await this.Client.PostAsync($"{BaseUrl}/product/categories/update/{productId}/{categoryId}", null);
        response.EnsureSuccessStatusCode();
    }
}