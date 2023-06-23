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
    public bool HideName { get; set; }
    
    [Parameter]
    public bool HideBrand { get; set; }

    [Parameter]
    public bool HideCategory { get; set; }

    [Parameter]
    public bool HideDate { get; set; }

    [Parameter]
    public bool HideStore { get; set; }

    [Parameter]
    public bool HideDetails { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool Virtualize { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private async Task ShowProductDetails(Product? product)
    {
        if (this.DialogService == null || product == null)
        {
            return;
        }
        
        var parameters = new DialogParameters { { "Product", product } };
        var options = new DialogOptions()
                      {
                          CloseButton = true,
                      };

        await this.DialogService.ShowAsync<ProductDetails>(product.Name, parameters, options);
    }
}