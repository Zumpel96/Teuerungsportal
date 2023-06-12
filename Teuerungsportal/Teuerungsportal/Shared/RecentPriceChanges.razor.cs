namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Newtonsoft.Json;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class RecentPriceChanges
{
    [Parameter]
    [EditorRequired]
    public ICollection<Price> PriceChanges { get; set; } = new List<Price>();

    [Parameter]
    [EditorRequired]
    public int NumberOfPages { get; set; }

    [Parameter]
    [EditorRequired]
    public int Page { get; set; }

    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    [Parameter]
    public bool HideCategory { get; set; }

    [Parameter]
    public bool HideDate { get; set; }

    [Parameter]
    public bool HidePagination { get; set; }

    [Parameter]
    public bool HideStore { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }


    [Inject]
    private CategoryService? CategoryService { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }


    private ICollection<Category> AllCategories { get; set; } = new List<Category>();
    
    private Category? SelectedCategory { get; set; }

    private bool IsCategorizing { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.CategoryService == null)
        {
            return;
        }

        this.AllCategories = await this.CategoryService.GetCategories();
        this.AllCategories = this.AllCategories.OrderBy(c => c.Name).ToList();
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

    private void Redirect(Product? product)
    {
        if (this.NavigationManager == null)
        {
            return;
        }

        if (product?.Store == null)
        {
            return;
        }

        this.NavigationManager.NavigateTo($"/stores/{product.Store.Name}/{product.ArticleNumber}");
    }

    private async Task OnPageChanged(int i)
    {
        this.Page = i;
        await this.PageChanged.InvokeAsync(this.Page);
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
        newUncategorized.SubCategories.FirstOrDefault(nsc => oldUncategorizedCopy.SubCategories.FirstOrDefault(osc => osc.Id == nsc.Id) == null);

        return newUncategorizedCategory;
    }
}