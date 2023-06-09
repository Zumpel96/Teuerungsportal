namespace Teuerungsportal.Pages;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
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
    private InflationDataService? InflationDataService { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }
    
    [Inject]
    private ILocalStorageService? LocalStorageService { get; set; }

    [Inject]
    private AnnouncementService? AnnouncementService { get; set; }
    
    private bool IsLoadingPriceData { get; set; }

    private int PricePages { get; set; }

    private int CurrentPricePage { get; set; }
    
    private Announcement? Announcement { get; set; }

    private ICollection<Price> TotalPriceHistory { get; set; } = new List<Price>();
    
    private ICollection<InflationData> InflationHistory { get; set; } = new List<InflationData>();
    
    private ICollection<InflationData> InflationOverview { get; set; } = new List<InflationData>();
    
    private ICollection<Price> WorstPriceChanges { get; set; } = new List<Price>();
    
    private ICollection<Price> TopPriceChanges { get; set; } = new List<Price>();
    
    private ICollection<Price> PaginatedPriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.PriceService == null)
        {
            return;
        }

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

        if (this.AnnouncementService == null)
        {
            return;
        }

        if (this.InflationDataService == null)
        {
            return;
        }

        this.IsLoadingPriceData = true;
        var cookieConsentThread = this.LocalStorageService.GetItemAsync<bool>("cookieConsent");
        var announcementThread = this.AnnouncementService.GetAnnouncement();

        this.InflationHistory = await this.InflationDataService.GetInflationDataForMonth();
        this.InflationOverview = await this.InflationDataService.GetInflationDataForYear();
        
        this.CurrentPricePage = 1;
        this.TotalPriceHistory = await this.PriceService.GetPriceChanges();
        this.TopPriceChanges = await this.PriceService.GetTopPriceChanges();
        this.WorstPriceChanges = await this.PriceService.GetWorstPriceChanges();

        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
        this.PricePages = (int)Math.Ceiling((float)this.TotalPriceHistory.Count / 25);

        this.Announcement = await announcementThread;
        var cookieConsentShown = await cookieConsentThread;
        if (!cookieConsentShown)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            await this.DialogService.ShowAsync<CookieConsent>(this.L["cookieConsent"], options);
            await this.LocalStorageService.SetItemAsync("cookieConsent", true);
        }

        this.IsLoadingPriceData = false;
    }

    private void OnPricePageChanged(int page)
    {
        this.CurrentPricePage = page;
        this.PaginatedPriceHistory = this.TotalPriceHistory.Skip((this.CurrentPricePage - 1) * 25).Take(25).ToList();
    }
}