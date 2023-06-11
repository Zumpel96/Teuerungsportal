namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Plotly.Blazor;
using Plotly.Blazor.ConfigLib;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Margin = Plotly.Blazor.LayoutLib.Margin;

public partial class InflationTransitionChart
{
    [Parameter]
    [EditorRequired]
    public ICollection<InflationData> ChartData { get; set; } = new List<InflationData>();

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private PlotlyChart? Chart { get; set; }

    private Config Config { get; set; } = new ()
                                          {
                                              Responsive = true,
                                              DisplayModeBar = DisplayModeBarEnum.False,
                                              ScrollZoom = ScrollZoomFlag.False
                                          };

    private Layout? Layout { get; set; } = new ()
                                           {
                                               DragMode = DragModeEnum.False,
                                               Height = 400,
                                               Margin = new Margin
                                                        {
                                                            L = 40,
                                                            B = 60,
                                                            R = 0,
                                                            T = 0,
                                                        },
                                           };

    private IList<ITrace> Data { get; set; } = new List<ITrace>();

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

        var storeData = this.ChartData.OrderBy(p => p.Date).GroupBy(p => p.Store!.Name).ToList();
        foreach (var store in storeData)
        {
            var x = new List<object>();
            var y = new List<object>();

            var previousValue = 1d;
            for (var i = 30; i >= 0; i--)
            {
                var newDate = DateTime.Today;
                newDate = newDate.AddDays(-i);

                x.Add(newDate.ToString("dd.MMM"));

                var foundValue = store.ToList().FirstOrDefault(p => p.Date.Date == newDate);
                if (foundValue == null)
                {
                    y.Add(previousValue * 100 - 100);
                }
                else
                {
                    var newValue = previousValue / 100 * (100 + foundValue.InflationValue);
                    previousValue = newValue;
                    y.Add(newValue * 100 - 100);
                }
            }

            this.Data.Add(
                          new Scatter()
                          {
                              X = x,
                              Y = y,
                              Name = store.Key,
                              ShowLegend = true,
                          });
        }
    }
}