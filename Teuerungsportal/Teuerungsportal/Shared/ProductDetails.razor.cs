namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class ProductDetails
{
    [Parameter]
    public Product Product { get; set; } = new ();

    [Inject]
    private PriceService? PriceService { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    private string Url { get; set; } = string.Empty;

    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        _ = this.LoadData();
    }

    private async Task LoadData()
    {
        if (this.PriceService == null  || this.ProductService == null || this.IsLoading)
        {
            return;
        }
        
        this.IsLoading = true;
        this.PriceHistory = await this.PriceService.GetProductPriceChanges(this.Product.Id);
        this.Url = await this.ProductService.GetProductUrl(this.Product.Id);
        this.IsLoading = false;
        
        this.StateHasChanged();
    }

    private void Submit() => this.MudDialog?.Close(DialogResult.Ok(true));
}