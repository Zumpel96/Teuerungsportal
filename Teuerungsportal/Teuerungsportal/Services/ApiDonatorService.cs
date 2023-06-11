namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiDonatorService : DonatorService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net";
    private HttpClient Client { get; set; }

    public ApiDonatorService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<ICollection<Donator>> GetDonators()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/donators");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<List<Donator>>(responseBody);

        return data ?? new List<Donator>();
    }
}