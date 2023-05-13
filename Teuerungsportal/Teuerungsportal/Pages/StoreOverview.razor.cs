namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class StoreOverview
{
    [Parameter]
    public string StoreName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private Store? CurrentStore { get; set; }

    private List<BreadcrumbItem> Breadcrumbs { get; set; } = new ();

    private ICollection<Price> RecentPriceChanges { get; set; } = new List<Price>();

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (this.L == null)
        {
            return;
        }

        this.CurrentStore = new Store()
                               {
                                   Name = "Store A",
                               };

        this.Breadcrumbs.Add(new BreadcrumbItem(this.L["overview"], $"stores"));
        this.Breadcrumbs.Add(new BreadcrumbItem(this.CurrentStore.Name, null, true));

        for (var i = 0; i < 10; i++)
        {
            this.RecentPriceChanges.Add(
                                        new Price()
                                        {
                                            Value = 1 + i * 2.1,
                                            LastValue = i == 5 ? null : 1.22 + i * 2,
                                            TimeStamp = DateTime.Now.AddMinutes(i),
                                            Product = new Product()
                                                      {
                                                          Brand = "Test",
                                                          Name = $"Test Product A-{i}",
                                                          ArticleNumber = "123456",
                                                          Store = new Store() { Name = "Billa" },
                                                          Url = "#",
                                                      },
                                        });
        }

        this.PriceHistory = new List<Price>();
        for (var i = 0; i < 30; i++)
        {
            this.PriceHistory.Add(
                                  new Price()
                                  {
                                      Value = 15 - i * 0.15,
                                      TimeStamp = DateTime.Now.AddDays(-i * 8),
                                      Product = new Product()
                                                {
                                                    Brand = "Test",
                                                    Name = $"Test Product A-{i}",
                                                    ArticleNumber = "123456",
                                                    Store = new Store() { Name = "Billa" },
                                                    Url = "#",
                                                },
                                  });
        }
    }
}