namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class CategoryOverview
{
    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private CategoryService? CategoryService { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    private Category CurrentCategory { get; set; } = new ();

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }


    private ICollection<Price> PriceChanges { get; set; } = new List<Price>();

    private int ProductPages { get; set; }

    private int CurrentProductPage { get; set; }

    private ICollection<Product> Products { get; set; } = new List<Product>();

    private ICollection<InflationData> InflationHistory { get; set; } = new List<InflationData>();
    
    private bool IsLoadingMeta { get; set; } = true;
    
    private bool IsLoadingPriceData { get; set; }

    private bool IsLoadingProductData { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.CategoryService == null)
        {
            return;
        }

        this.IsLoadingMeta = true;
        var loadedData = await this.CategoryService.GetCategory(this.CategoryName);
        if (loadedData == null)
        {
            return;
        }

        this.CurrentCategory = loadedData;

        this.ParentCategories = new List<BreadcrumbItem> { new (this.L["overview"], $"categories") };
        if (this.CurrentCategory.ParentCategory != null)
        {
            this.ParentCategories.Add(
                                      new BreadcrumbItem(
                                                         this.CurrentCategory.ParentCategory.Name,
                                                         $"categories/{this.CurrentCategory.ParentCategory.Name}"));
        }

        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentCategory.Name, null, true));

        this.ProductPages = await this.CategoryService.GetCategoryProductPages(this.CurrentCategory.Id);
        this.PricePages = await this.CategoryService.GetCategoryPriceChangesPages(this.CurrentCategory.Id);

        this.CurrentProductPage = 1;
        this.CurrentPricePage = 1;

        this.IsLoadingProductData = true;
        this.IsLoadingPriceData = true;
        
        var productThread = this.CategoryService.GetCategoryProducts(this.CurrentCategory.Id, this.CurrentProductPage);
        var pricesThread = this.CategoryService.GetCategoryPriceChanges(this.CurrentCategory.Id, this.CurrentPricePage);
        var chartThread = this.CategoryService.GetCategoryInflationData(this.CurrentCategory.Id);

        this.InflationHistory = await chartThread;
        this.IsLoadingMeta = false;
        
        this.Products = await productThread;
        this.IsLoadingProductData = false;
        
        this.PriceChanges = await pricesThread;
        this.IsLoadingPriceData = false;

        this.StateHasChanged();
    }

    private async Task OnProductPageChanged(int page)
    {
        if (this.CategoryService == null)
        {
            return;
        }
        
        this.CurrentProductPage = page;
        this.IsLoadingProductData = true;
        this.Products = await this.CategoryService.GetCategoryProducts(this.CurrentCategory.Id, this.CurrentProductPage);
        this.IsLoadingProductData = false;
    }

    private async Task OnPricePageChanged(int page)
    {
        if (this.CategoryService == null)
        {
            return;
        }

        this.CurrentPricePage = page;
        this.IsLoadingPriceData = true;
        this.PriceChanges = await this.CategoryService.GetCategoryPriceChanges(this.CurrentCategory.Id, this.CurrentPricePage);
        this.IsLoadingPriceData = false;
    }

    private void Redirect(Category category)
    {
        if (this.NavigationManager == null)
        {
            return;
        }

        this.NavigationManager.NavigateTo($"categories/{category.Name}");
    }
}