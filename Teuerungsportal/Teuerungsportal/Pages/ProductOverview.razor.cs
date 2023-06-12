namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;
using Teuerungsportal.Shared;

public partial class ProductOverview
{
    [Parameter]
    public string StoreName { get; set; } = string.Empty;

    [Parameter]
    public string ProductNumber { get; set; } = string.Empty;

    [Inject]
    private IDialogService? DialogService { get; set; }

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

    private Category? SelectedCategory { get; set; }

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    private bool IsLoading { get; set; }

    private bool IsCategorizing { get; set; }

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

    private async Task AddCategory(Product product, Category? category)
    {
        this.SelectedCategory = null;
        if (category?.Name == "newCategory")
        {
            category = await this.CreateNewCategory();
        }
        
        if (this.ProductService == null)
        {
            return;
        }

        if (category == null || category.Id == Guid.Empty)
        {
            return;
        }

        this.IsCategorizing = true;
        await this.ProductService.UpdateProductCategory(product.Id, category.Id);
        product.Category = category;
        this.IsCategorizing = false;
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

    private async Task<Category?> CreateNewCategory()
    {
        if (this.CategoryService == null)
        {
            return null;
        }

        if (this.DialogService == null)
        {
            return null;
        }

        if (this.L == null)
        {
            return null;
        }

        var dialog = await this.DialogService.ShowAsync<CategoryCreation>(this.L["createCategory"]);
        await dialog.Result;

        var oldUncategorized = this.AllCategories.FirstOrDefault(c => c.Id == new Guid("23b3d57b-7d2f-4544-b4d3-3b7fdbdd22f8"));
        var oldUncategorizedCopy = JsonConvert.DeserializeObject<Category?>(JsonConvert.SerializeObject(oldUncategorized));

        this.AllCategories = await this.CategoryService.GetCategories();
        this.AllCategories = this.AllCategories.OrderBy(c => c.Name).ToList();

        var newUncategorized = this.AllCategories.FirstOrDefault(c => c.Id == new Guid("23b3d57b-7d2f-4544-b4d3-3b7fdbdd22f8"));

        if (newUncategorized == null || oldUncategorizedCopy == null)
        {
            return null;
        }

        if (newUncategorized.SubCategories.Count == oldUncategorizedCopy.SubCategories.Count)
        {
            return null;
        }

        var newUncategorizedCategory =
        newUncategorized.SubCategories.FirstOrDefault(
                                                      nsc => oldUncategorizedCopy.SubCategories.FirstOrDefault(osc => osc.Id == nsc.Id) ==
                                                             null);

        return newUncategorizedCategory;
    }
}