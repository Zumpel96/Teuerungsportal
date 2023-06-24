namespace Teuerungsportal.Services;

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
    public async Task<ICollection<Price>> GetNewProducts(int page, string filter, IEnumerable<string> storeNames)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/new/{page}/{filter}/{string.Join("+", storeNames)}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<FilteredCount>> GetProductSearchCounts(string searchString)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/total/search/{searchString}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<FilteredCount>>(responseBody);

        return data ?? new List<FilteredCount>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetProductsSearch(int page, string searchString, string filter, IEnumerable<string> storeNames)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/products/search/{searchString}/{page}/{filter}/{string.Join("+", storeNames)}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
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