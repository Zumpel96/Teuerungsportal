namespace Teuerungsportal.Services;

using System.Net;
using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiProductService : ProductService
{
    private const string BaseUrl = "https://api.teuerungsportal.at";
    private HttpClient Client { get; set; }

    public ApiProductService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<int> GetProductsWithoutCategoryPages()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/noCategory");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Product>> GetProductsWithoutCategory(int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/noCategory/{page}");
        
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Product>>(responseBody);

        return data ?? new List<Product>();
    }

    /// <inheritdoc />
    public async Task<Product?> GetProduct(string store, string productNumber)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/{productNumber}/store/{store}");

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
    public async Task<int> GetProductPriceChangesPages(Guid productId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/{productId}/prices");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetProductPriceChanges(Guid productId, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/{productId}/prices/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<int> GetProductSearchPages(string searchString)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/search/{searchString}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Product>> GetProductsSearch(string searchString, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/search/{searchString}/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Product>>(responseBody);

        return data ?? new List<Product>();
    }

    /// <inheritdoc />
    public async Task UpdateProductCategory(Guid productId, Guid categoryId)
    {
        var response = await this.Client.PostAsync($"{BaseUrl}/product/categories/update/{productId}/{categoryId}", null);
        response.EnsureSuccessStatusCode();
    }
}