namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Store
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Color { get; set; }

    public string BaseUrl { get; set; }

    public Store()
    {
        this.Name = string.Empty;
        this.BaseUrl = string.Empty;
        this.Color = "#FFFFFF";
    }
    
    public Store(StoreDbo dbo)
    {
        this.Id = dbo.Id;
        this.Name = dbo.Name;
        this.Color = dbo.Color;
        this.BaseUrl = dbo.BaseUrl;
    }
}