namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;

public partial class InflationOverviewChart
{
    [Parameter]
    [EditorRequired]
    public ICollection<InflationData> ChartData { get; set; } = new List<InflationData>();

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

        var today = DateTime.Today;
        for (var i = 12; i >= 0; i--)
        {
            var newDate = new DateTime(today.Year, today.Month, 1);
            dateLabels.Add(newDate.AddMonths(-i));
        }

        var storeData = this.ChartData.OrderBy(p => p.Date).GroupBy(p => p.Store!.Name).ToList();
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
        this.Labels = dateLabels.Select(d => d.ToString("MMM")).ToArray();
        this.ChartOptions.YAxisTicks = 1;
        this.ChartOptions.MaxNumYAxisTicks = 5;
        this.ChartOptions.InterpolationOption = InterpolationOption.NaturalSpline;
    }

    private List<double> ProcessChartData(ICollection<InflationData> prices, ICollection<DateTime> dates)
    {
        var chartTotalData = new List<double>(12);
        foreach (var date in dates)
        {
            var foundEntry = prices.FirstOrDefault(d => d.Date == date);
            if (foundEntry == null)
            {
                chartTotalData.Add(0);
                continue;
            }

            chartTotalData.Add(foundEntry.InflationValue);
        }

        return chartTotalData;
    }
}