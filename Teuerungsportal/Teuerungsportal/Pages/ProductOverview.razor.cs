namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class ProductOverview
{
    [Parameter]
    public string StoreName { get; set; } = string.Empty;

    [Parameter]
    public string ProductNumber { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    private Product CurrentProduct { get; set; } = new ();

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();
    
    private bool IsLoadingMeta { get; set; }
    
    private bool IsLoadingPrices { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.ProductService == null)
        {
            return;
        }

        if (this.PriceService == null)
        {
            return;
        }

        this.IsLoadingMeta = true;
        this.IsLoadingPrices = true;

        var loadedProduct = await this.ProductService.GetProduct(this.StoreName, this.ProductNumber);
        this.IsLoadingMeta = false;
        
        if (loadedProduct?.Store == null || loadedProduct.Category == null)
        {
            return;
        }

        this.CurrentProduct = loadedProduct;
        
        this.ParentCategories.Add(new BreadcrumbItem(this.L["overview"], $"stores"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Store.Name, $"stores/{this.CurrentProduct.Store.Name}"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Category.Name, $"categories/{this.CurrentProduct.Category.Name}"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Name, null, true));

        this.PriceHistory = await this.PriceService.GetPriceChangesForProduct(this.CurrentProduct.Id);
        this.PriceHistory = this.PriceHistory.OrderBy(p => p.TimeStamp).ToList();
        double lastValue = 0;
        
        foreach (var price in this.PriceHistory)
        {
            price.Product = this.CurrentProduct;
            price.LastValue = lastValue == 0 ? null : lastValue;

            lastValue = price.Value;
        }

        this.PriceHistory = this.PriceHistory.OrderByDescending(p => p.TimeStamp).ToList();
        this.IsLoadingPrices = false;
    }
}