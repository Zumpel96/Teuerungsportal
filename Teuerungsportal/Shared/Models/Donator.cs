namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Donator
{
    public string Name { get; set; } = string.Empty;

    public Donator()
    {
    }

    public Donator(DonatorDbo dbo)
    {
        this.Name = dbo.Name;
    }
}