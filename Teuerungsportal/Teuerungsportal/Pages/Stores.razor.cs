namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Stores
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private StoreService? StoreService { get; set; }

    private ICollection<Store> StoresList { get; set; } = new List<Store>();
    
    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.StoreService == null)
        {
            return;
        }

        this.IsLoading = true;
        this.StoresList = await this.StoreService.GetStores();
        this.IsLoading = false;
    }
}