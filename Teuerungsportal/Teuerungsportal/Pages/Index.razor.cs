namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Index
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    private bool IsLoadingPriceData { get; set; }

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }

    private ICollection<Price> TotalPriceHistory { get; set; } = new List<Price>();
    
    private ICollection<Price> PaginatedPriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.PriceService == null)
        {
            return;
        }

        this.IsLoadingPriceData = true;
        this.CurrentPricePage = 1;
        this.TotalPriceHistory = await this.PriceService.GetPriceChanges();

        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
        this.PricePages = (int)Math.Ceiling((float)this.TotalPriceHistory.Count / 25);
        this.IsLoadingPriceData = false;
    }

    private void OnPricePageChanged(int page)
    {
        this.CurrentPricePage = page;
        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
    }
}