namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Resources;

public partial class NavMenu
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ILocalStorageService? LocalStorage { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    private async Task ChangeLanguage(string culture)
    {
        if (this.LocalStorage == null)
        {
            return;
        }

        if (this.NavigationManager == null)
        {
            return;
        }
        
        await this.LocalStorage.SetItemAsync<string>("culture", culture);
        this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
    }
}