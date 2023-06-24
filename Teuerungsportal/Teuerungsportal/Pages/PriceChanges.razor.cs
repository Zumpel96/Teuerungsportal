namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Teuerungsportal.Helpers;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class PriceChanges
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    [Inject]
    private IJSRuntime? JSRuntime { get; set; }

    private bool IsLoadingCountData { get; set; }

    private bool IsLoadingProductData { get; set; }

    private int CurrentProductPage { get; set; }

    private OrderType OrderType { get; set; }

    private ICollection<FilteredCount> FilteredCount { get; set; } = new List<FilteredCount>();

    private IList<bool> ActiveFilters { get; set; } = new List<bool>();

    private ICollection<Price> TotalNewProducts { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (this.JSRuntime == null)
        {
            return;
        }

        if (firstRender)
        {
            await this.JSRuntime.InvokeVoidAsync("onDivScroll", DotNetObjectReference.Create(this));
        }
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.PriceService == null)
        {
            return;
        }

        this.CurrentProductPage = 1;

        await this.LoadCounts();
        await this.LoadProducts();
    }

    private async Task LoadCounts()
    {
        if (this.PriceService == null || this.IsLoadingCountData)
        {
            return;
        }

        this.IsLoadingCountData = true;
        this.StateHasChanged();

        this.FilteredCount = await this.PriceService.GetAllPriceChanges("/day");

        this.ActiveFilters = new List<bool>();
        foreach (var unused in this.FilteredCount)
        {
            this.ActiveFilters.Add(true);
        }

        this.IsLoadingCountData = false;
        this.StateHasChanged();
    }

    private async Task LoadProducts()
    {
        if (this.PriceService == null || this.IsLoadingProductData)
        {
            return;
        }

        this.IsLoadingProductData = true;
        this.StateHasChanged();

        var newProducts = this.OrderType switch
                          {
                              OrderType.Grouped => await this.PriceService.GetTodayPriceChanges(
                                                                                                this.CurrentProductPage,
                                                                                                "store",
                                                                                                this.GetActiveStores()),
                              OrderType.Descending => await this.PriceService.GetTodayPriceChanges(
                                                                                                   this.CurrentProductPage,
                                                                                                   "descending",
                                                                                                   this.GetActiveStores()),           
                              OrderType.PriceChange => await this.PriceService.GetTodayPriceChanges(
                                                                                                   this.CurrentProductPage,
                                                                                                   "change",
                                                                                                   this.GetActiveStores()),
                              _ => await this.PriceService.GetTodayPriceChanges(
                                                                                this.CurrentProductPage,
                                                                                "ascending",
                                                                                this.GetActiveStores()),
                          };

        this.TotalNewProducts = this.TotalNewProducts.Concat(newProducts).ToList();
        this.IsLoadingProductData = false;

        this.StateHasChanged();
    }

    private async Task ToggleFilter(int index)
    {
        this.ActiveFilters[index] = !this.ActiveFilters[index];
        this.CurrentProductPage = 1;
        this.TotalNewProducts = new List<Price>();

        await this.LoadProducts();
    }

    private async Task ChangeOrderType(OrderType orderType)
    {
        this.OrderType = orderType;
        this.CurrentProductPage = 1;
        this.TotalNewProducts = new List<Price>();

        await this.LoadProducts();
    }

    [JSInvokable]
    public async Task LoadMoreData()
    {
        if (this.PriceService == null)
        {
            return;
        }

        if (this.IsLoadingProductData)
        {
            return;
        }

        var activeSum = 0d;
        for (var i = 0; i < this.FilteredCount.Count; i++)
        {
            if (!this.ActiveFilters.ElementAt(i))
            {
                continue;
            }

            activeSum += this.FilteredCount.ElementAt(i).Count;
        }

        if (this.TotalNewProducts.Count >= activeSum)
        {
            return;
        }

        this.CurrentProductPage++;
        await this.LoadProducts();
    }

    private ICollection<string> GetActiveStores()
    {
        var activeStores = new List<string>();

        for (var i = 0; i < this.FilteredCount.Count; i++)
        {
            if (!this.ActiveFilters[i])
            {
                continue;
            }

            activeStores.Add(this.FilteredCount.ElementAt(i).StoreName);
        }

        return activeStores;
    }
}