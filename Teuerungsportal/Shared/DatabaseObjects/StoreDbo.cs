namespace Shared.DatabaseObjects;

public class StoreDbo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;
}