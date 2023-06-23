namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Utilities;
using Teuerungsportal.Resources;

public partial class MainLayout
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }
    
    private bool DrawerOpen { get; set; } = true;

    private MudTheme CustomTheme { get; set; } = new ();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this.CustomTheme = new MudTheme()
                           {
                               Typography = new Typography()
                                            {
                                                Default = new Default()
                                                          {
                                                              FontFamily = new[] { "Madefor", "Helvetica", "Arial", "sans-serif" },
                                                          },
                                                H1 = new H1
                                                     {
                                                         FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
                                                         FontSize = "2.5rem",
                                                         FontWeight = 500,
                                                         LineHeight = 1.6,
                                                         LetterSpacing = ".0075em",
                                                     },
                                                H2= new H2
                                                    {
                                                        FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
                                                        FontSize = "1.75rem",
                                                        FontWeight = 450,
                                                        LineHeight = 1.2,
                                                        LetterSpacing = ".0055em",
                                                    },
                                                H3 = new H3
                                                     {
                                                         FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
                                                         FontSize = "1.25rem",
                                                         FontWeight = 400,
                                                         LineHeight = 1,
                                                         LetterSpacing = ".0045em",
                                                     },
                                            },
                           };
    }

    private void DrawerToggle()
    {
        this.DrawerOpen = !this.DrawerOpen;
    }
}