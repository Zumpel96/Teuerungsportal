namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Helpers;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Categories
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private CategoryService? CategoryService { get; set; }

    private ICollection<(Category Category, int Count)> CategoriesList { get; set; } = new List<(Category Category, int Count)>();

    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.CategoryService == null)
        {
            return;
        }

        this.IsLoading = true;
        var allCategories = await this.CategoryService.GetCategoriesWithChildren();
        foreach (var category in allCategories)
        {
            //var numberOfProducts = await this.CategoryService.GetNumberOfProducts(category.Id);
            this.CategoriesList.Add((category, 0));
        }
        
        this.IsLoading = false;
    }
}