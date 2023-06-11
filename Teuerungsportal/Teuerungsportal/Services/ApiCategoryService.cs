namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiCategoryService : CategoryService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net";
    private HttpClient Client { get; set; }

    public ApiCategoryService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<Category?> GetCategory(string categoryName)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryName}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Category>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Category>> GetCategories()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Category>>(responseBody);
        return data == null ? new List<Category>() : data.OrderBy(c => c.Name).ToList();
    }

    /// <inheritdoc />
    public async Task<ICollection<Category>> GetUngroupedCategories()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/ungrouped");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Category>>(responseBody);
        return data == null ? new List<Category>() : data.OrderBy(c => c.Name).ToList();
    }

    /// <inheritdoc />
    public async Task<int> GetCategoryProductPages(Guid categoryId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryId}/products");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Product>> GetCategoryProducts(Guid categoryId, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryId}/products/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Product>>(responseBody);

        return data ?? new List<Product>();
    }

    /// <inheritdoc />
    public async Task<int> GetCategoryPriceChangesPages(Guid categoryId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryId}/prices");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetCategoryPriceChanges(Guid categoryId, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryId}/prices/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<InflationData>> GetCategoryInflationData(Guid categoryId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories/{categoryId}/chart");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<InflationData>>(responseBody);

        return data ?? new List<InflationData>();
    }
}