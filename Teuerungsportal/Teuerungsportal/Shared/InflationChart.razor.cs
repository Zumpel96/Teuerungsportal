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

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private ChartOptions ChartOptions { get; set; } = new ();

    private List<ChartSeries> ProcessedChartData { get; set; } = new ();

    private string[] Labels { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (this.L == null)
        {
            return;
        }

        var dateLabels = new List<DateTime>();
        var chartSeries = new List<ChartSeries>();
        var storeLabels = new List<string>();

        for (var i = 11; i >= 0; i--)
        {
            var today = DateTime.Today;
            dateLabels.Add(new DateTime(today.Year, today.Month, 1).AddMonths(-i));
        }

        var storeData = this.ChartData.OrderBy(p => p.TimeStamp).GroupBy(p => p.Product!.Store!.Name).ToList();
        if (storeData.Count > 1)
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

        var totalData = this.ProcessChartData(this.ChartData, dateLabels);
        chartSeries.Add(
                        new ChartSeries()
                        {
                            Name = this.L["total"],
                            Data = totalData.ToArray(),
                        });

        this.ProcessedChartData = chartSeries;
        this.Labels = dateLabels.Select(d => d.ToString("MM.yyyy")).ToArray();
    }

    private List<double> ProcessChartData(ICollection<Price> prices, ICollection<DateTime> dates)
    {
        var orderedTotalData = prices.OrderBy(v => v.TimeStamp).
                                      GroupBy(
                                              v => new
                                                   {
                                                       v.TimeStamp.Year,
                                                       v.TimeStamp.Month,
                                                   }).
                                      Select(
                                             cv => new
                                                   {
                                                       Date = new DateTime(cv.Key.Year, cv.Key.Month, 1),
                                                       AveragePrice = cv.Average(p => p.Value),
                                                   }).
                                      ToList();

        var currentTotalValue = orderedTotalData.First().AveragePrice;
        var chartTotalData = new List<double>(12);
        foreach (var date in dates)
        {
            var foundEntry = orderedTotalData.FirstOrDefault(d => d.Date == date);
            if (foundEntry == null)
            {
                chartTotalData.Add(currentTotalValue);
                continue;
            }

            currentTotalValue = foundEntry.AveragePrice;
            chartTotalData.Add(foundEntry.AveragePrice);
        }

        return chartTotalData;
    }
}