namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class CategoryOverview
{
    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    [Inject]
    private CategoryService? CategoryService { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    private Category? CurrentCategory { get; set; }

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();
    
    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();
    
    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.CategoryService == null)
        {
            return;
        }

        if (this.PriceService == null)
        {
            return;
        }

        this.IsLoading = true;
        var loadedData = await this.CategoryService.GetCategory(this.CategoryName);
        if (loadedData == null)
        {
            return;
        }

        this.CurrentCategory = loadedData;

        this.ParentCategories = new List<BreadcrumbItem> { new (this.L["overview"], $"categories") };
        foreach (var parentCategory in this.CurrentCategory.ParentCategories)
        {
            this.ParentCategories.Add(new BreadcrumbItem(parentCategory.Name, $"categories/{parentCategory.Name}"));
        }
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentCategory.Name, null, true));

        this.PriceHistory = await this.PriceService.GetPriceChangesForCategory(this.CurrentCategory.Id);
        this.PriceHistory = this.PriceHistory.OrderBy(p => p.TimeStamp).ToList();
        double lastValue = 0;

        foreach (var price in this.PriceHistory)
        {
            price.LastValue = lastValue == 0 ? null : lastValue;
            lastValue = price.Value;
        }

        this.PriceHistory = this.PriceHistory.OrderByDescending(p => p.TimeStamp).ToList();
        this.StateHasChanged();
        this.IsLoading = false;
    }
}