namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Plotly.Blazor;
using Plotly.Blazor.ConfigLib;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.LayoutLib.LegendLib;
using Plotly.Blazor.LayoutLib.YAxisLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib.LineLib;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Margin = Plotly.Blazor.LayoutLib.Margin;
using TypeEnum = Plotly.Blazor.LayoutLib.XAxisLib.TypeEnum;

public partial class PriceTransitionChart
{
    [Parameter]
    [EditorRequired]
    public ICollection<Price> PriceData { get; set; } = new List<Price>();

    [Parameter]
    [EditorRequired]
    public string ProductName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private PlotlyChart? Chart { get; set; }

    private Config Config { get; set; } = new ()
                                          {
                                              Responsive = true,
                                              DisplayModeBar = DisplayModeBarEnum.False,
                                              ScrollZoom = ScrollZoomFlag.False,
                                          };

    private Layout? Layout { get; set; } = new ()
                                           {
                                               DragMode = DragModeEnum.False,
                                               Height = 400,
                                               Margin = new Margin
                                                        {
                                                            L = 50,
                                                            R = 0,
                                                            T = 0,
                                                            B = 60,
                                                            Pad = 16,
                                                        },
                                               Legend = new Legend
                                                        {
                                                            Orientation = OrientationEnum.H,
                                                        },
                                               XAxis = new List<XAxis>()
                                                       {
                                                           new ()
                                                           {
                                                               DTick = 7,
                                                           },
                                                       },
                                               YAxis = new List<YAxis>()
                                                       {
                                                           new ()
                                                           {
                                                               TickSuffix = "€",
                                                               RangeMode = RangeModeEnum.ToZero,
                                                           },
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

        if (!this.PriceData.Any())
        {
            return;
        }

        var x = new List<object>();
        var y = new List<object>();

        var prices = this.PriceData.OrderBy(p => p.TimeStamp).ToList();
        var currentDate = prices.First().TimeStamp.Date;
        var previousValue = prices.First().CurrentValue;

        do
        {
            x.Add(currentDate.ToString("dd.MMM"));
            var foundValue = prices.LastOrDefault(p => p.TimeStamp.Date == currentDate);
            if (foundValue == null)
            {
                y.Add(previousValue);
            }
            else
            {
                previousValue = foundValue.CurrentValue;
                y.Add(foundValue.CurrentValue);
            }

            currentDate = currentDate.AddDays(1);
        }
        while (currentDate != DateTime.Today.AddDays(1));

        this.Data.Add(
                      new Scatter()
                      {
                          X = x,
                          Y = y,
                          Marker = new ()
                                   {
                                       Color = "#F1A208",
                                   },
                          Line = new ()
                                 {
                                     Color = "#F1A208",
                                 },
                          Name = this.ProductName,
                          ShowLegend = false,
                      });
    }
}