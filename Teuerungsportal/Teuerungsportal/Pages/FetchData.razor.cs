namespace Teuerungsportal.Pages;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

public partial class FetchData
{
    [Inject]
    private HttpClient? HttpClient { get; set; }
    
    private WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (this.HttpClient == null)
        {
            return;
        }
        
        this.Forecasts = await this.HttpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }

        public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
    }
}