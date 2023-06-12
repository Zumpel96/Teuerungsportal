namespace Teuerungsportal.Shared;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class CategoryCreation
{
    public class CategoryModel
    {
        [Required]
        [StringLength(32, MinimumLength = 4)]
        public string Name { get; set; } = string.Empty;
    }
    
    [Parameter]
    public EventCallback CategoryNameChanged { get; set; }
    
    [Inject]
    private IStringLocalizer<Language>? L { get; set; }

    [Inject]
    private CategoryService? CategoryService { get; set; }

    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }

    private CategoryModel Model { get; set; } = new ();

    private async Task Submit()
    {
        if (this.CategoryService == null)
        {
            this.MudDialog?.Close(DialogResult.Ok(false));
            return;
        }

        await this.CategoryService.AddCategory(this.Model.Name);
        this.MudDialog?.Close(DialogResult.Ok(true));
    } 

    private void Cancel() => this.MudDialog?.Close(DialogResult.Cancel());
}