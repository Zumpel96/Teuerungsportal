namespace Teuerungsportal.Shared;

using Blazored.LocalStorage;
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

    private bool IsDarkTheme { get; set; }

    private MudTheme CustomTheme { get; set; } = new ();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this.CustomTheme = new MudTheme()
                           {
                               Palette = new PaletteLight()
                                         {
                                             DrawerBackground = "#191716",
                                             Background = "#F5F5F5",
                                             DrawerText = "#E1E1E1",
                                             DrawerIcon = "#C9082A",
                                             Primary = "#C9082A",
                                             Error = "#C9082A",
                                             TextPrimary = "#191716",
                                             Secondary = "#06A77D",
                                             Tertiary = "#F1A208",
                                             Success = "#06A77D",
                                             Warning = "#F1A208",
                                             AppbarBackground = "#F5F5F5",
                                             Surface = new MudColor(255, 255, 255, 0.33),
                                         },
                               PaletteDark = new PaletteDark()
                                             {
                                                 DrawerBackground = "#191716",
                                                 Background = "#1c1c1c",
                                                 DrawerText = "#E1E1E1",
                                                 DrawerIcon = "#C9082A",
                                                 Primary = "#C9082A",
                                                 Error = "#C9082A",
                                                 TextPrimary = "#E1E1E1",
                                                 Secondary = "#06A77D",
                                                 Tertiary = "#F1A208",
                                                 Success = "#06A77D",
                                                 Warning = "#F1A208",
                                                 AppbarBackground = "#191716",
                                                 Surface = new MudColor(25, 23, 22, 0.66),
                                             },
                               Typography = new Typography()
                                            {
                                                Default =
                                                new Default()
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
                                                H2 = new H2
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