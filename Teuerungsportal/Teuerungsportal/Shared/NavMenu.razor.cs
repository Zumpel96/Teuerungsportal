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
    public class SearchModel
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Text { get; set; } = string.Empty;
    }
    
    [Parameter]
    public bool DrawerState { get; set; }
    
    [Parameter]
    public EventCallback<bool> DrawerStateChanged { get; set; }

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ILocalStorageService? LocalStorage { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }
    
    private SearchModel Search { get; set; } = new ();

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
        
        await this.LocalStorage.SetItemAsync("culture", culture);
        this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
    }
    
    private void Redirect()
    {
        if (this.NavigationManager == null)
        {
            return;
        }

        this.NavigationManager.NavigateTo($"search/{this.Search.Text}");
        this.Search = new SearchModel();
    }

    private async Task ToggleDrawer()
    {
        this.DrawerState = !this.DrawerState;
        await this.DrawerStateChanged.InvokeAsync(this.DrawerState);
    }
}