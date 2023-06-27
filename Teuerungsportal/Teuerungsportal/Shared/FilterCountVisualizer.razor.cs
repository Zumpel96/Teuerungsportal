namespace Teuerungsportal.Shared;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Teuerungsportal.Models;
using Teuerungsportal.Resources;
using Teuerungsportal.Services.Interfaces;

public partial class FilterCountVisualizer
{
    [Parameter]
    public bool IsLoading { get; set; }
    
    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    [EditorRequired]
    public FilteredCount FilteredCount { get; set; } = new ();
    
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task Clicked()
    {
        await this.OnClick.InvokeAsync();
    } 
}