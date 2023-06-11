namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Uncategorized
{
    public class SearchModel
    {
        public string Text { get; set; } = string.Empty;
    }

    [Parameter]
    public string? SearchString { get; set; }
    
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

    private SearchModel Search { get; set; } = new ();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.ProductService == null)
        {
            return;
        }

        this.IsLoading = true;
        this.Page = 1;

        if (string.IsNullOrEmpty(this.SearchString))
        {
            this.ProductList = await this.ProductService.GetProductsWithoutCategory(this.Page);
            this.NumberOfPages = await this.ProductService.GetProductsWithoutCategoryPages();
        }
        else
        {
            this.Search = new SearchModel() { Text = this.SearchString };
            this.ProductList = await this.ProductService.GetProductsWithoutCategorySearch(this.SearchString, this.Page);
            this.NumberOfPages = await this.ProductService.GetProductsWithoutCategorySearchPages(this.SearchString);
        }
        
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
        this.ProductList = await this.ProductService.GetProductsWithoutCategory(this.Page);
        this.IsLoading = false;
    }

    private void SearchRedirect()
    {
        if (this.NavigationManager == null)
        {
            return;
        }

        this.ProductList = new List<Product>();
        if (this.Search.Text == string.Empty)
        {
            this.NavigationManager.NavigateTo($"products/uncategorized");
            return;
        }

        this.NavigationManager.NavigateTo($"products/uncategorized/{this.Search.Text}");
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