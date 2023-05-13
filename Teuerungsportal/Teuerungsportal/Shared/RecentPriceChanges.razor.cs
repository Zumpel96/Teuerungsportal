namespace Teuerungsportal.Shared;

using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class RecentPriceChanges
{
    [Parameter]
    public ICollection<Price> PriceChanges { get; set; } = new List<Price>();

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        this.PriceChanges = this.PriceChanges.OrderByDescending(pc => pc.TimeStamp).ToList();
    }
}