namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
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
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }


    [Inject]
    private CategoryService? CategoryService { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }


    private ICollection<Category> AllCategories { get; set; } = new List<Category>();

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
}