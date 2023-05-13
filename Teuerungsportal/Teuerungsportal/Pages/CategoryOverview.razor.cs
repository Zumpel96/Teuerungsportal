namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class CategoryOverview
{
    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private Category? CurrentCategory { get; set; }

    private List<BreadcrumbItem> ParentCategories { get; set; } = new ();

    private ICollection<Price> RecentPriceChanges { get; set; } = new List<Price>();

    private ICollection<Price> PriceHistory { get; set; } = new List<Price>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (this.L == null)
        {
            return;
        }

        this.CurrentCategory = new Category()
                               {
                                   Name = "Test B-C",
                                   SubCategories = new List<Category>()
                                                   {
                                                       new () { Name = "Test B-C-A" },
                                                       new () { Name = "Test B-C-B" },
                                                       new () { Name = "Test B-C-C" },
                                                   },
                                   ParentCategories = new List<Category>()
                                                      {
                                                          new () { Name = "Test B" },
                                                      }
                               };

        this.ParentCategories.Add(new BreadcrumbItem(this.L["overview"], $"categories"));

        foreach (var parentCategory in this.CurrentCategory.ParentCategories)
        {
            this.ParentCategories.Add(new BreadcrumbItem(parentCategory.Name, $"categories/{parentCategory.Name}"));
        }

        this.ParentCategories.Add(new BreadcrumbItem(this.CurrentCategory.Name, null, true));

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
                                                          Store = "Billa",
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
                                                    Store = "Billa",
                                                    Url = "#",
                                                },
                                  });
        }
    }
}