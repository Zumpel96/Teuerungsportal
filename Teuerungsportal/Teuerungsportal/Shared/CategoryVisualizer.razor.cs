namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;
using Teuerungsportal.Models;

public partial class CategoryVisualizer
{
    [Parameter]
    [EditorRequired]
    public Category Category { get; set; } = new ();

    [Parameter]
    [EditorRequired]
    public int RecursionLevel { get; set; }

    private string RecursionClass
    {
        get
        {
            return this.RecursionLevel switch
                   {
                       0 => "select-heading-item",
                       1 => "select-sub-heading-item",
                       _ => "select-text-item",
                   };
        }
    }

    private string RecursionPadding => $"padding-left: {1.0 * this.RecursionLevel}rem!important;";
}