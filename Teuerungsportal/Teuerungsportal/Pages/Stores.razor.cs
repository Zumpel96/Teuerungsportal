namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class Stores
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private ICollection<Store> StoresList { get; set; } = new List<Store>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this.StoresList.Add(new Store() { Name = "Store A" });
        this.StoresList.Add(new Store() { Name = "Store B" });
        this.StoresList.Add(new Store() { Name = "Store C" });
    }
}