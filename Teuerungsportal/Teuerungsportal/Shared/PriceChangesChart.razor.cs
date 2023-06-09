namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;

public partial class PriceChangesChart
{
    [Parameter]
    [EditorRequired]
    public ICollection<Price> ChartData { get; set; } = new List<Price>();

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private ChartOptions ChartOptions { get; set; } = new ();

    private List<ChartSeries> ProcessedChartData { get; set; } = new ();

    private string[] Labels { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (this.L == null)
        {
            return;
        }

        if (!this.ChartData.Any())
        {
            return;
        }

        var dateLabels = new List<DateTime>();
        var chartSeries = new List<ChartSeries>();
        var storeLabels = new List<string>();

        for (var i = 30; i >= 0; i--)
        {
            dateLabels.Add(DateTime.Today.AddDays(-i));
        }

        var storeData = this.ChartData.OrderBy(p => p.TimeStamp).GroupBy(p => p.Product!.Store!.Name).ToList();
        if (storeData.Count >= 1)
        {
            foreach (var store in storeData)
            {
                if (!storeLabels.Contains(store.Key))
                {
                    storeLabels.Add(store.Key);
                }

                var storePrices = store.ToList();
                var processedStoreData = this.ProcessChartData(storePrices, dateLabels);

                chartSeries.Add(
                                new ChartSeries()
                                {
                                    Name = store.Key,
                                    Data = processedStoreData.ToArray(),
                                });
            }
        }
        
        this.ProcessedChartData = chartSeries;
        this.Labels = dateLabels.Select(d => d.ToString("dd")).ToArray();
        this.ChartOptions.YAxisTicks = 1;
        this.ChartOptions.MaxNumYAxisTicks = 5;
        this.ChartOptions.InterpolationOption = InterpolationOption.NaturalSpline;
    }

    private List<double> ProcessChartData(ICollection<Price> prices, ICollection<DateTime> dates)
    {
        var orderedTotalData = prices.OrderBy(v => v.TimeStamp).
                                      GroupBy(
                                              v => new
                                                   {
                                                       v.TimeStamp.Year,
                                                       v.TimeStamp.Month,
                                                       v.TimeStamp.Day,
                                                   }).
                                      Select(
                                             cv => new
                                                   {
                                                       Date = new DateTime(cv.Key.Year, cv.Key.Month, cv.Key.Day),
                                                       AverageInflation = cv.Average(p => 100 / p.PreviousValue * p.CurrentValue) ?? 100,
                                                   }).
                                      ToList();
        
        var chartTotalData = new List<double>(12);
        foreach (var date in dates)
        {
            var foundEntry = orderedTotalData.FirstOrDefault(d => d.Date == date);
            if (foundEntry == null)
            {
                chartTotalData.Add(100);
                continue;
            }

            chartTotalData.Add(foundEntry.AverageInflation);
        }

        return chartTotalData;
    }
}