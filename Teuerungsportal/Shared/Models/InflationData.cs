namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class InflationData
{
    public double InflationValue { get; set; }

    public DateTime Date { get; set; }

    public Store? Store { get; set; }

    public InflationData()
    {
    }

    public InflationData(InflationDataDbo dbo)
    {
        this.InflationValue = dbo.InflationValue;
        this.Date = dbo.Date;

        this.Store = new Store()
                     {
                         Id = dbo.StoreId,
                         Name = dbo.StoreName,
                         Color = dbo.StoreColor,
                     };
    }
}