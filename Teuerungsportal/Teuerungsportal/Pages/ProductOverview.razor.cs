namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class ProductOverview
{
    [Parameter]
    public string StoreName { get; set; } = string.Empty;

    [Parameter]
    public string ProductNumber { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private ProductService? ProductService { get; set; }

    private Product CurrentProduct { get; set; } = new ();

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();

    private ICollection<Price> RecentPriceChanges { get; set; } = new List<Price>();

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.L == null)
        {
            return;
        }

        if (this.ProductService == null)
        {
            return;
        }

        var loadedProduct = await this.ProductService.GetProduct(this.StoreName, this.ProductNumber);
        if (loadedProduct?.Store == null || loadedProduct.Category == null)
        {
            return;
        }

        this.CurrentProduct = loadedProduct;
        
        this.ParentCategories.Add(new BreadcrumbItem(this.L["overview"], $"stores"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Store.Name, $"stores/{this.CurrentProduct.Store.Name}"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Category.Name, $"categories/{this.CurrentProduct.Category.Name}"));
        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentProduct.Name, null, true));

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
                                                          Name = $"Test Product",
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
                                                    Name = $"Test Product",
                                                    ArticleNumber = "123456",
                                                    Store = new Store() { Name = "Billa" },
                                                    Url = "#",
                                                },
                                  });
        }
    }
}