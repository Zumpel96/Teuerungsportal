namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;

public partial class ProductTable
{
    [Parameter]
    [EditorRequired]
    public ICollection<Product> Products { get; set; } = new List<Product>();

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
    public bool HideStore { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

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