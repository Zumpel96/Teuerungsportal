namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Teuerungsportal.Helpers;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class StoreOverview
{
    [Parameter]
    public string StoreName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private InflationDataService? InflationDataService { get; set; }

    [Inject]
    private IJSRuntime? JSRuntime { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    [Inject]
    private StoreService? StoreService { get; set; }

    private Store CurrentStore { get; set; } = new ();

    private ICollection<InflationData> InflationHistory { get; set; } = new List<InflationData>();


    private ICollection<Price> TotalPriceChanges { get; set; } = new List<Price>();

    private bool IsLoadingPriceHistory { get; set; }

    private bool IsLoadingProductsCounts { get; set; }

    private bool IsLoadingPriceChangesCount { get; set; }

    private bool IsLoadingInflationData { get; set; }

    private bool IsLoadingProductData { get; set; }

    private bool FinishedLoading { get; set; }

    private string SelectedFilter { get; set; } = "/day";

    private OrderType OrderType { get; set; }

    private int CurrentProductPage { get; set; }

    private FilteredCount ProductsCounts { get; set; } = new ();

    private FilteredCount PriceChangesCounts { get; set; } = new ();

    private FilteredCount InflationData { get; set; } = new ();

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
        if (this.StoreService == null)
        {
            return;
        }

        this.IsLoadingInflationData = true;
        this.IsLoadingProductData = true;
        this.IsLoadingPriceHistory = true;
        this.IsLoadingProductsCounts = true;
        this.IsLoadingPriceChangesCount = true;
        
        var loadedStore = await this.StoreService.GetStore(this.StoreName);

        this.IsLoadingInflationData = false;
        this.IsLoadingProductData = false;
        this.IsLoadingPriceHistory = false;
        this.IsLoadingProductsCounts = false;
        this.IsLoadingPriceChangesCount = false;
        
        if (loadedStore == null)
        {
            return;
        }

        this.CurrentProductPage = 1;
        this.CurrentStore = loadedStore;

        _ = this.LoadProductCounts();
        _ = this.LoadPriceChangesCount();
        _ = this.LoadInflationData();
        _ = this.LoadProducts();
        _ = this.LoadPriceHistory();
    }

    private async Task LoadProductCounts()
    {
        if (this.ProductService == null || this.IsLoadingProductsCounts)
        {
            return;
        }

        this.IsLoadingProductsCounts = true;
        this.ProductsCounts = await this.ProductService.GetStoreProductCounts(this.SelectedFilter, this.CurrentStore.Id);
        this.IsLoadingProductsCounts = false;
        this.StateHasChanged();
    }

    private async Task LoadPriceChangesCount()
    {
        if (this.PriceService == null || this.IsLoadingPriceChangesCount)
        {
            return;
        }

        this.IsLoadingPriceChangesCount = true;
        this.PriceChangesCounts = await this.PriceService.GetStorePriceChangesCount(this.SelectedFilter, this.CurrentStore.Id);
        this.IsLoadingPriceChangesCount = false;
        this.StateHasChanged();
    }

    private async Task LoadInflationData()
    {
        if (this.InflationDataService == null || this.IsLoadingInflationData)
        {
            return;
        }

        this.IsLoadingInflationData = true;
        this.InflationData = await this.InflationDataService.GetStoreInflationData(this.SelectedFilter, this.CurrentStore.Id);
        this.IsLoadingInflationData = false;
        this.StateHasChanged();
    }

    private async Task LoadPriceHistory()
    {
        if (this.InflationDataService == null || this.IsLoadingPriceHistory)
        {
            return;
        }

        this.IsLoadingPriceHistory = true;
        this.InflationHistory = await this.InflationDataService.GetInflationDataForStore(this.CurrentStore.Id);
        this.IsLoadingPriceHistory = false;
        this.StateHasChanged();
    }

    [JSInvokable]
    public async Task LoadMoreData()
    {
        if (this.ProductService == null)
        {
            return;
        }

        if (this.IsLoadingProductData)
        {
            return;
        }

        if (this.FinishedLoading)
        {
            return;
        }

        var oldCount = this.TotalPriceChanges.Count;
        this.CurrentProductPage++;
        await this.LoadProducts();
        var newCount = this.TotalPriceChanges.Count;
        this.FinishedLoading = oldCount == newCount;
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
                              OrderType.Ascending => await this.PriceService.GetStorePriceChanges(
                                                                                                  "/ascending",
                                                                                                  this.CurrentProductPage,
                                                                                                  this.CurrentStore.Id),
                              OrderType.Descending => await this.PriceService.GetStorePriceChanges(
                                                                                                  "/descending",
                                                                                                  this.CurrentProductPage,
                                                                                                  this.CurrentStore.Id),                              
                              OrderType.PriceChange => await this.PriceService.GetStorePriceChanges(
                                                                                                  "/change",
                                                                                                  this.CurrentProductPage,
                                                                                                  this.CurrentStore.Id),
                              _ => await this.PriceService.GetStorePriceChanges(
                                                                                "/time",
                                                                                this.CurrentProductPage,
                                                                                this.CurrentStore.Id),
                          };

        this.TotalPriceChanges = this.TotalPriceChanges.Concat(newProducts).ToList();
        this.IsLoadingProductData = false;

        this.StateHasChanged();
    }

    private void FilterChanged(string newValue)
    {
        this.SelectedFilter = newValue;
        _ = this.LoadProductCounts();
        _ = this.LoadPriceChangesCount();
        _ = this.LoadInflationData();
    }

    private async Task ChangeOrderType(OrderType orderType)
    {
        this.OrderType = orderType;
        this.CurrentProductPage = 1;
        this.TotalPriceChanges = new List<Price>();

        await this.LoadProducts();
    }
}