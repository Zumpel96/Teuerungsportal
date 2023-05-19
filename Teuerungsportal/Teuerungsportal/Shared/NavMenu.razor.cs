namespace Teuerungsportal.Shared;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    
    private string Search { get; set; } = string.Empty;

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
    
    private void Redirect()
    {
        if (this.NavigationManager == null)
        {
            return;
        }
        
        this.NavigationManager.NavigateTo($"search/{this.Search}");
    }
}