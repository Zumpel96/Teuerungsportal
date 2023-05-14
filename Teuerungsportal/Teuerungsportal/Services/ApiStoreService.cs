namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Helpers;
using Teuerungsportal.Services.Interfaces;


public class ApiStoreService : StoreService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net/api";
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
}