namespace Teuerungsportal.Services;

using System.Net;
using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiProductService : ProductService
{
    private const string BaseUrl = "https://api.teuerungsportal.at/v2";
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
    public async Task<ICollection<Price>> GetProductsWithoutCategory(int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/noCategory/{page}");
        
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<int> GetProductsWithoutCategorySearchPages(string searchString)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/noCategory/search/{searchString}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetProductsWithoutCategorySearch(string searchString, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/noCategory/search/{searchString}/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Price>>(responseBody);

        return data ?? new List<Price>();
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
    public async Task<int> GetNewProductsPages()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/new");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetNewProducts(int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/new/{page}");

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
    public async Task<ICollection<Price>> GetProductsSearch(string searchString, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/search/{searchString}/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task UpdateProductCategory(Guid productId, Guid categoryId)
    {
        var response = await this.Client.PostAsync($"{BaseUrl}/products/{productId}/category/{categoryId}", null);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task<ICollection<FilteredCount>> GetAllProductCounts(string? filter)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/total{filter}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<FilteredCount>>(responseBody);

        return data ?? new List<FilteredCount>();
    }
}