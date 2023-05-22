namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Resources;

public partial class CookieConsent
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [CascadingParameter]
    MudDialogInstance? MudDialog { get; set; }

    void Submit() => this.MudDialog?.Close(DialogResult.Ok(true));
}