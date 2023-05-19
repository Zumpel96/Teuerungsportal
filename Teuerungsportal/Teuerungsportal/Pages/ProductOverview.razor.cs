namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
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
    private CategoryService? CategoryService { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    private Product CurrentProduct { get; set; } = new ();

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();

    private ICollection<Category> AllCategories { get; set; } = new List<Category>();

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    private string SelectedCategoryName { get; set; } = string.Empty;

    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.ProductService == null)
        {
            return;
        }
        
        if (this.CategoryService == null)
        {
            return;
        }

        this.IsLoading = true;

        var loadedProduct = await this.ProductService.GetProduct(this.StoreName, this.ProductNumber);
        if (loadedProduct?.Store == null)
        {
            return;
        }

        this.CurrentProduct = loadedProduct;
        this.PricePages = await this.CategoryService.GetCategoryPriceChangesPages(this.CurrentProduct.Id);
        this.CurrentPricePage = 1;

        if (this.CurrentProduct.Category == null)
        {
            this.AllCategories = await this.CategoryService.GetCategories();
        }

        this.ParentCategories.Add(new BreadcrumbItem(this.L["overview"], $"stores"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Store.Name, $"stores/{this.CurrentProduct.Store.Name}"));

        if (this.CurrentProduct.Category != null)
        {
            this.ParentCategories.Add(
                                      new BreadcrumbItem(
                                                         this.CurrentProduct.Category.Name,
                                                         $"categories/{this.CurrentProduct.Category.Name}"));
        }

        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Name, null, true));

        this.PricePages = await this.ProductService.GetProductPriceChangesPages(this.CurrentProduct.Id);
        this.CurrentPricePage = 1;
        this.PriceHistory = await this.ProductService.GetProductPriceChanges(this.CurrentProduct.Id, this.CurrentPricePage);
        
        this.IsLoading = false;
    }

    private Task<IEnumerable<string>> Search(string value)
    {
        return Task.FromResult(
                               this.AllCategories.Where(c => c.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)).
                                    Select(c => c.Name));
    }

    private async Task SubmitCategory()
    {
        if (this.ProductService == null)
        {
            return;
        }

        if (this.NavigationManager == null)
        {
            return;
        }

        var category = this.AllCategories.FirstOrDefault(
                                                         c => string.Equals(
                                                                            c.Name,
                                                                            this.SelectedCategoryName,
                                                                            StringComparison.CurrentCultureIgnoreCase));

        if (category == null)
        {
            return;
        }

        await this.ProductService.UpdateProductCategory(this.CurrentProduct.Id, category.Id);
        this.NavigationManager.NavigateTo($"/stores/{this.StoreName}/{this.ProductNumber}", true);
    }

    private async Task OnPricePageChanged(int page)
    {
        if (this.ProductService == null)
        {
            return;
        }

        this.CurrentPricePage = page;
        this.IsLoading = true;
        this.PriceHistory = await this.ProductService.GetProductPriceChanges(this.CurrentProduct.Id, this.CurrentPricePage);
        this.IsLoading = false;
    }
}