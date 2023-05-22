namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Impress
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }
    
    [Inject]
    private DonatorService? DonatorService { get; set; }

    private ICollection<Donator> Donators { get; set; } = new List<Donator>();
    
    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.DonatorService == null)
        {
            return;
        }

        this.IsLoading = true;
        this.Donators = await this.DonatorService.GetDonators();
        this.IsLoading = false;
    }
}