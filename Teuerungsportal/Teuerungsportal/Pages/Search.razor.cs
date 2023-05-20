namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Search
{
    [Parameter]
    public string SearchString { get; set; } = string.Empty;
    
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    private ICollection<Product> ProductList { get; set; } = new List<Product>();

    private bool IsLoading { get; set; }

    private int Page { get; set; }
    
    private int NumberOfPages { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.ProductService == null)
        {
            return;
        }

        if (this.SearchString == string.Empty)
        {
            return;
        }

        this.IsLoading = true;
        this.Page = 1;

        this.NumberOfPages = await this.ProductService.GetProductSearchPages(this.SearchString);
        this.ProductList = await this.ProductService.GetProductsSearch(this.SearchString, this.Page);
        
        this.IsLoading = false;
    }

    private async Task OnPageChanged(int i)
    {
        if (this.ProductService == null)
        {
            return;
        }
        
        this.Page = i;
        this.IsLoading = true;
        this.ProductList = await this.ProductService.GetProductsSearch(this.SearchString, this.Page);
        this.IsLoading = false;
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
}