namespace Teuerungsportal.Shared;

using Microsoft.AspNetCore.Components;

public partial class BlockQuote
{
    [Parameter]
    public string Text { get; set; }
    
    [Parameter]
    public string Quote { get; set; }
    
    [Parameter]
    public string Url { get; set; }
}