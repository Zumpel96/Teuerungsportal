namespace Teuerungsportal.Pages;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Resources;
using Teuerungsportal.Shared;

public partial class Index
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private ILocalStorageService? LocalStorageService { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.DialogService == null)
        {
            return;
        }

        if (this.L == null)
        {
            return;
        }

        if (this.LocalStorageService == null)
        {
            return;
        }

        var cookieConsentShown = await this.LocalStorageService.GetItemAsync<bool>("cookieConsent");
        if (!cookieConsentShown)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            await this.DialogService.ShowAsync<CookieConsent>(this.L["cookieConsent"], options);
            await this.LocalStorageService.SetItemAsync("cookieConsent", true);
        }
    }
}