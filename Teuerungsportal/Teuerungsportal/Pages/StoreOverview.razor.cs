namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
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
    private StoreService? StoreService { get; set; }

    private Store CurrentStore { get; set; } = new ();

    private List<BreadcrumbItem> Breadcrumbs { get; set; } = new ();

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }

    private ICollection<Price> PriceChanges { get; set; } = new List<Price>();

    private int ProductPages { get; set; }

    private int CurrentProductPage { get; set; }

    private ICollection<Product> Products { get; set; } = new List<Product>();

    private ICollection<InflationData> InflationHistory { get; set; } = new List<InflationData>();

    private bool IsLoadingMeta { get; set; }
    
    private bool IsLoadingPriceData { get; set; }
    
    private bool IsLoadingProductData { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.StoreService == null)
        {
            return;
        }

        this.IsLoadingMeta = true;
        var loadedStore = await this.StoreService.GetStore(this.StoreName);
        if (loadedStore == null)
        {
            return;
        }

        this.CurrentStore = loadedStore;
        this.Breadcrumbs.Add(new BreadcrumbItem(this.L["overview"], $"stores"));
        this.Breadcrumbs.Add(new BreadcrumbItem(this.CurrentStore.Name, null, true));

        this.ProductPages = await this.StoreService.GetStoreProductsPages(this.CurrentStore.Id);
        this.PricePages = await this.StoreService.GetStorePriceChangesPages(this.CurrentStore.Id);

        this.CurrentProductPage = 1;
        this.CurrentPricePage = 1;

        this.IsLoadingPriceData = true;
        this.IsLoadingProductData = true;

        var productThread = this.StoreService.GetStoreProducts(this.CurrentStore.Id, this.CurrentProductPage);
        var pricesThread = this.StoreService.GetStorePriceChanges(this.CurrentStore.Id, this.CurrentPricePage);
        var chartThread = this.StoreService.GetStoreInflationData(this.CurrentStore.Id);

        this.InflationHistory = await chartThread;
        this.IsLoadingMeta = false;
        
        this.Products = await productThread;
        this.IsLoadingProductData = false;

        this.PriceChanges = await pricesThread;
        this.IsLoadingPriceData = false;
    }

    private async Task OnProductPageChanged(int page)
    {
        if (this.StoreService == null)
        {
            return;
        }
        
        this.CurrentProductPage = page;
        this.IsLoadingProductData = true;
        this.Products = await this.StoreService.GetStoreProducts(this.CurrentStore.Id, this.CurrentProductPage);
        this.IsLoadingProductData = false;
    }

    private async Task OnPricePageChanged(int page)
    {
        if (this.StoreService == null)
        {
            return;
        }

        this.CurrentPricePage = page;
        this.IsLoadingPriceData = true;
        this.PriceChanges = await this.StoreService.GetStorePriceChanges(this.CurrentStore.Id, this.CurrentPricePage);
        this.IsLoadingPriceData = false;
    }
}