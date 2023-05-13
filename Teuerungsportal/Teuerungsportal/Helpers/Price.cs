namespace Teuerungsportal.Helpers;

public class Price
{
    public int Id { get; set; }

    public double Value { get; set; }

    public double? LastValue { get; set; }

    public DateTime TimeStamp { get; set; }

    public Product? Product { get; set; }
}