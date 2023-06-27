namespace Teuerungsportal.Pages;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;
using Teuerungsportal.Shared;

public partial class Index
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private PriceService? PriceService { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    [Inject]
    private InflationDataService? InflationDataService { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private ILocalStorageService? LocalStorageService { get; set; }

    private ICollection<InflationData> InflationHistory { get; set; } = new List<InflationData>();

    private ICollection<Price> WorstPriceChanges { get; set; } = new List<Price>();

    private ICollection<Price> TopPriceChanges { get; set; } = new List<Price>();

    private bool IsLoadingProductsCounts { get; set; }

    private bool IsLoadingPriceChangesCount { get; set; }

    private bool IsLoadingPriceHistory { get; set; }

    private bool IsLoadingPriceChanges { get; set; }
    
    private bool IsLoadingInflationData { get; set; }

    private bool ShowDecreasesData { get; set; } = true;

    private string SelectedFilter { get; set; } = "/day";

    private ICollection<FilteredCount> ProductsCounts { get; set; } = new List<FilteredCount>();

    private ICollection<FilteredCount> PriceChangesCounts { get; set; } = new List<FilteredCount>();

    private ICollection<FilteredCount> InflationData { get; set; } = new List<FilteredCount>();


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

        _ = this.LoadProductCounts();
        _ = this.LoadPriceChangesCount();
        _ = this.LoadInflationData();
        _ = this.LoadPriceHistory();
        _ = this.LoadPriceChanges();
    }

    private async Task LoadProductCounts()
    {
        if (this.ProductService == null || this.IsLoadingProductsCounts)
        {
            return;
        }

        this.IsLoadingProductsCounts = true;
        this.ProductsCounts = await this.ProductService.GetAllProductCounts(this.SelectedFilter);
        this.IsLoadingProductsCounts = false;
        this.StateHasChanged();
    }

    private async Task LoadPriceChangesCount()
    {
        if (this.PriceService == null || this.IsLoadingPriceChangesCount)
        {
            return;
        }

        this.IsLoadingPriceChangesCount = true;
        this.PriceChangesCounts = await this.PriceService.GetAllPriceChanges(this.SelectedFilter);
        this.IsLoadingPriceChangesCount = false;
        this.StateHasChanged();
    }

    private async Task LoadInflationData()
    {
        if (this.InflationDataService == null || this.IsLoadingInflationData)
        {
            return;
        }

        this.IsLoadingInflationData = true;
        this.InflationData = await this.InflationDataService.GetInflationData(this.SelectedFilter);
        this.IsLoadingInflationData = false;
        this.StateHasChanged();
    }

    private async Task LoadPriceHistory()
    {
        if (this.InflationDataService == null || this.IsLoadingPriceHistory)
        {
            return;
        }

        this.IsLoadingPriceHistory = true;
        this.InflationHistory = await this.InflationDataService.GetInflationDataForMonth();
        this.IsLoadingPriceHistory = false;
        this.StateHasChanged();
    }

    private async Task LoadPriceChanges()
    {
        if (this.PriceService == null || this.IsLoadingPriceChanges)
        {
            return;
        }

        this.IsLoadingPriceChanges = true;
        this.TopPriceChanges = await this.PriceService.GetTopPriceChanges();
        this.WorstPriceChanges = await this.PriceService.GetWorstPriceChanges();
        this.IsLoadingPriceChanges = false;
        this.StateHasChanged();
    }

    private void ChangeTopData(bool isDecreaseTab)
    {
        this.ShowDecreasesData = isDecreaseTab;
    }

    private void FilterChanged(string newValue)
    {
        this.SelectedFilter = newValue;
        _ = this.LoadProductCounts();
        _ = this.LoadPriceChangesCount();
        _ = this.LoadInflationData();
    }
}