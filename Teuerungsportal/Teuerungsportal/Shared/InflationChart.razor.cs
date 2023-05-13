namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class InflationChart
{
    [Parameter]
    [EditorRequired]
    public ICollection<Price> ChartData { get; set; } = new List<Price>();

    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private ChartOptions ChartOptions { get; set; } = new ();

    private List<ChartSeries> ProcessedChartData { get; set; } = new ();

    private string[] Labels { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        var labels = new List<string>();

        var orderedData = this.ChartData.OrderBy(v => v.TimeStamp).
                               GroupBy(
                                       v => new
                                            {
                                                v.TimeStamp.Year,
                                                v.TimeStamp.Month,
                                            }).
                               Select(
                                      cv => new
                                            {
                                                Key = cv.Key,
                                                AveragePrice = cv.Average(p => p.Value),
                                            });

        var chartData = new List<double>();
        foreach (var entry in orderedData)
        {
            var date = new DateTime(entry.Key.Year, entry.Key.Month, 1);
            labels.Add($"{date:MM.yyyy}");
            chartData.Add(entry.AveragePrice);
        }

        this.ProcessedChartData.Add(
                                    new ChartSeries()
                                    {
                                        Name = this.Label,
                                        Data = chartData.ToArray()
                                    });

        this.Labels = labels.ToArray();
    }
}