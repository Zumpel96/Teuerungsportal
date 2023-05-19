namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Store
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string BaseUrl { get; set; }

    public Store()
    {
        this.Name = string.Empty;
        this.BaseUrl = string.Empty;
    }
    
    public Store(StoreDbo dbo)
    {
        this.Id = dbo.Id;
        this.Name = dbo.Name;
        this.BaseUrl = dbo.BaseUrl;
    }
}