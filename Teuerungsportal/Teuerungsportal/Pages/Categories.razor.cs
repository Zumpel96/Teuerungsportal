namespace Teuerungsportal.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class Categories
{
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private CategoryService? CategoryService { get; set; }

    private ICollection<Category> CategoriesList { get; set; } = new List<Category>();

    private bool IsLoading { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (this.CategoryService == null)
        {
            return;
        }

        this.IsLoading = true;
        this.CategoriesList = await this.CategoryService.GetCategories();
        this.IsLoading = false;
    }
}