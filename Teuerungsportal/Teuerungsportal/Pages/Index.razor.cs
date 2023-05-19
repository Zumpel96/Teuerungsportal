namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
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

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.PriceService == null)
        {
            return;
        }

        this.IsLoadingPriceData = true;
        this.CurrentPricePage = 1;
        this.PricePages = await this.PriceService.GetPriceChangesPages();
        this.PriceHistory = await this.PriceService.GetPriceChanges(this.CurrentPricePage);
        this.IsLoadingPriceData = false;
    }

    private async Task OnPricePageChanged(int page)
    {
        if (this.PriceService == null)
        {
            return;
        }

        this.CurrentPricePage = page;
        this.IsLoadingPriceData = true;
        this.PriceHistory = await this.PriceService.GetPriceChanges(this.CurrentPricePage);
        this.IsLoadingPriceData = false;
    }
}