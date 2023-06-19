namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class NewProducts
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    private bool IsLoadingProductData { get; set; }

    private int ProductPages { get; set; }

    private int CurrentProductPage { get; set; }

    private ICollection<Price> TotalNewProducts { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.ProductService == null)
        {
            return;
        }

        this.IsLoadingProductData = true;

        this.ProductPages = await this.ProductService.GetNewProductsPages();
        this.CurrentProductPage = 1;
        this.TotalNewProducts = await this.ProductService.GetNewProducts(this.CurrentProductPage);

        this.IsLoadingProductData = false;
    }

    private async Task OnProductPageChanged(int page)
    {
        if (this.ProductService == null)
        {
            return;
        }

        this.CurrentProductPage = page;
        this.TotalNewProducts = await this.ProductService.GetNewProducts(this.CurrentProductPage);
    }
}