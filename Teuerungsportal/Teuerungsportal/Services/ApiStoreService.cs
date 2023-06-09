namespace Teuerungsportal.Services;

using System.Net;
using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;


public class ApiStoreService : StoreService
{
    private const string BaseUrl = "https://api.teuerungsportal.at";
    private HttpClient Client { get; set; }
    
    public ApiStoreService(HttpClient client)
    {
        this.Client = client;
    }
    
    /// <inheritdoc />
    public async Task<ICollection<Store>> GetStores()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Store>>(responseBody);

        return data ?? new List<Store>();
    }

    /// <inheritdoc />
    public async Task<Store?> GetStore(string name)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{name}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Store>(responseBody);

        return data ?? null;
    }

    /// <inheritdoc />
    public async Task<int> GetStoreProductsPages(Guid storeId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{storeId}/products");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<int> GetStorePriceChangesPages(Guid storeId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{storeId}/prices");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);

        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Product>> GetStoreProducts(Guid storeId, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{storeId}/products/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Product>>(responseBody);

        return data ?? new List<Product>();
    }

    /// <inheritdoc />
    public async Task<ICollection<Price>> GetStorePriceChanges(Guid storeId, int page)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{storeId}/prices/{page}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Price>>(responseBody);

        return data ?? new List<Price>();
    }

    /// <inheritdoc />
    public async Task<ICollection<InflationData>> GetStoreInflationData(Guid storeId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/stores/{storeId}/chart");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<InflationData>>(responseBody);

        return data ?? new List<InflationData>();
    }
}