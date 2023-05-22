namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Services.Interfaces;

public class ApiAnnouncementService : AnnouncementService
{
    private const string BaseUrl = "https://api.teuerungsportal.at";
    private HttpClient Client { get; set; }

    public ApiAnnouncementService(HttpClient client)
    {
        this.Client = client;
    }
    
    /// <inheritdoc />
    public async Task<Announcement?> GetAnnouncement()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/announcement");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<Announcement>(responseBody);

        if (string.IsNullOrEmpty(data?.ContentDe) || string.IsNullOrEmpty(data?.ContentEn))
        {
            return null;
        }
        
        return data;
    }
}