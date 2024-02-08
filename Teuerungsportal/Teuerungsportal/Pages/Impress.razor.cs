namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Resources;

public partial class Impress
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }
}