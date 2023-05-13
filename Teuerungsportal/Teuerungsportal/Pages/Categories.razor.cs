namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;

public partial class Categories
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    private ICollection<Category> CategoriesList { get; set; } = new List<Category>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this.CategoriesList.Add(new Category() { Name = "Test A" });
        this.CategoriesList.Add(
                                new Category()
                                {
                                    Name = "Test B",
                                    SubCategories = new List<Category>()
                                                    {
                                                        new () { Name = "Test B-A" },
                                                        new () { Name = "Test B-B" },
                                                        new ()
                                                        {
                                                            Name = "Test B-C",
                                                            SubCategories = new List<Category>()
                                                                            {
                                                                                new ()
                                                                                {
                                                                                    Name = "Test B-C-A",
                                                                                },
                                                                                new ()
                                                                                {
                                                                                    Name = "Test B-C-B",
                                                                                },
                                                                                new ()
                                                                                {
                                                                                    Name = "Test B-C-C",
                                                                                },
                                                                            }
                                                        },
                                                    },
                                });
        this.CategoriesList.Add(
                                new Category()
                                {
                                    Name = "Test C",
                                    SubCategories = new List<Category>()
                                                    {
                                                        new () { Name = "Test C-A" },
                                                        new () { Name = "Test C-B" },
                                                        new () { Name = "Test C-C" },
                                                    },
                                });
    }
}