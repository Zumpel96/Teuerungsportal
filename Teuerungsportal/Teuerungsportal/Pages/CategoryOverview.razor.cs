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

    private ICollection<Price> TotalPriceHistory { get; set; } = new List<Price>();
    
    private ICollection<Price> PaginatedPriceHistory { get; set; } = new List<Price>();

    private int ProductPages { get; set; }

    private int CurrentProductPage { get; set; }

    private ICollection<Product> Products { get; set; } = new List<Product>();
    
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

        this.CurrentProductPage = 1;
        this.CurrentPricePage = 1;

        this.IsLoadingProductData = true;
        this.IsLoadingPriceData = true;
        
        var productThread = this.CategoryService.GetCategoryProducts(this.CurrentCategory.Id, this.CurrentProductPage);
        var pricesThread = this.CategoryService.GetCategoryPriceChanges(this.CurrentCategory.Id);

        this.Products = await productThread;
        this.IsLoadingProductData = false;
        
        this.TotalPriceHistory = await pricesThread;
        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
        this.PricePages = (int)Math.Ceiling((float)this.TotalPriceHistory.Count / 25);
        this.IsLoadingPriceData = false;

        this.StateHasChanged();
        this.IsLoadingMeta = false;
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

    private void OnPricePageChanged(int page)
    {
        this.CurrentPricePage = page;
        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
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