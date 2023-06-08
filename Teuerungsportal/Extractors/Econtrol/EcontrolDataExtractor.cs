namespace Api.Extractors.Econtrol;

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class Product
{
    public string name { get; set; }

    public string articleNumber { get; set; }

    public string url { get; set; }

    public string brand { get; set; }

    public Guid storeId { get; set; }
    
    public Guid categoryId { get; set; }
}

public class Price
{
    public double value { get; set; }

    public Guid productId { get; set; }
}

public class EcontrolDataExtractor
{
    private const string EcontrolStoreId = "fe733623-f9c4-4952-9e57-d18d0492a91f";
    
    private const string CategoryId = "2c360bc9-641f-496a-ba2d-e42b290a9cb9";

    private const string ProductIdCommand = "SELECT TOP(1) id FROM [dbo].[product] WHERE storeId=@storeId AND articleNumber=@articleNumber";

    private const string RecentPriceCommand = "SELECT TOP(1) value FROM [dbo].[price] WHERE productId=@productId ORDER BY timeStamp DESC";

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private string ZipCode { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<Product> DbProducts { get; set; }

    private IAsyncCollector<Price> DbPrices { get; set; }

    public EcontrolDataExtractor(string zipCode, IAsyncCollector<Product> dbProducts, IAsyncCollector<Price> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = "https://www.e-control.at/o/rc-public-rest/rate-calculator/energy-type/POWER/rate";
        this.ZipCode = zipCode;

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run()
    {
        // Initiate http call
        var content = new StringContent(
                                        $"{{\"customerGroup\":\"HOME\",\"energyType\":\"POWER\",\"zipCode\":{this.ZipCode},\"gridOperatorId\":10601,\"gridAreaId\":651,\"moveHome\":false,\"includeSwitchingDiscounts\":true,\"firstMeterOptions\":{{\"standardConsumption\":3000,\"smartMeterRequestOptions\":{{\"smartMeterSearch\":false,\"loadProfileUpload\":false,\"loadProfileId\":null,\"loadProfileName\":null,\"consumptionType\":null,\"calculatedValues\":null,\"detailedValues\":null,\"lastUploadDate\":null}}}},\"comparisonOptions\":{{\"brandId\":4651,\"mainProductId\":null,\"additionalProductId\":null,\"feedInProductId\":null,\"mainProductAssociationId\":null,\"additionalProductAssociationId\":null,\"feedInProductAssociationId\":null,\"productName\":null,\"manualEntry\":false,\"mainBaseRate\":null,\"mainEnergyRate\":null}},\"membership\":null,\"requirements\":[],\"priceView\":\"CENT_PER_YEAR\",\"referencePeriod\":\"ONE_YEAR\",\"includeFeedInOptions\":false,\"searchPriceModel\":\"CLASSIC\"}}",
                                        Encoding.UTF8,
                                        "application/json");
        var response = await this.Client.PostAsync(this.Url, content);

        // Handle the http response
        var json = await response.Content.ReadAsStringAsync();
        dynamic responseData = JsonConvert.DeserializeObject(json);

        if (responseData == null)
        {
            return;
        }
        
        var sum = 0f;
        var count = 0;

        // Iterate over Data
        foreach (var hit in responseData.ratedProducts)
        {
            var price = hit["averageTotalPriceInCentKWh"];
            float.TryParse(price.ToString(), out float newVal);

            if (newVal == 0)
            {
                return;
            }

            newVal = newVal / 100;
            sum += newVal;
            count++;
        }

        var articleNumber = $"ECAPAT{this.ZipCode}";

        // Create Product if it does not exist
        var existingProductId = await this.GetProduct(articleNumber);
        if (existingProductId == null)
        {
            var newProduct = new Product()
                             {
                                 name = $"Durchschnitt Strom {this.ZipCode}",
                                 articleNumber = articleNumber,
                                 url = string.Empty,
                                 brand = string.Empty,
                                 storeId = new Guid(EcontrolStoreId),
                                 categoryId = new Guid(CategoryId),
                             };

            await this.DbProducts.AddAsync(newProduct);
            await this.DbProducts.FlushAsync();
        }

        // Fetch Product
        existingProductId = await this.GetProduct(articleNumber);
        if (existingProductId == null)
        {
            return;
        }

        // Check if Price has Changed
        var newPriceValue = Math.Round(sum / count, 2);
        var recentPriceValue = await this.GetRecentPrice((Guid)existingProductId);
        if (Math.Round(recentPriceValue, 2) == newPriceValue)
        {
            return;
        }

        // Create new Price
        var newPrice = new Price()
                       {
                           value = newPriceValue,
                           productId = (Guid)existingProductId,
                       };

        await this.DbPrices.AddAsync(newPrice);
        await this.DbPrices.FlushAsync();
    }

    private async Task<Guid?> GetProduct(string articleNumber)
    {
        var existsCommand = new SqlCommand(ProductIdCommand, this.SqlConnection);
        existsCommand.Parameters.AddWithValue("@storeId", EcontrolStoreId);
        existsCommand.Parameters.AddWithValue("@articleNumber", articleNumber);

        await this.SqlConnection.OpenAsync();

        await using var existsReader = await existsCommand.ExecuteReaderAsync();
        var existingProductId = Guid.Empty;
        while (existsReader.Read())
        {
            existingProductId = new Guid(existsReader["id"].ToString() ?? string.Empty);
        }

        await this.SqlConnection.CloseAsync();
        return existingProductId == Guid.Empty ? null : existingProductId;
    }

    private async Task<double> GetRecentPrice(Guid productId)
    {
        var recentPriceCommand = new SqlCommand(RecentPriceCommand, this.SqlConnection);
        recentPriceCommand.Parameters.AddWithValue("@productId", productId);

        await this.SqlConnection.OpenAsync();

        await using var recentPriceReader = await recentPriceCommand.ExecuteReaderAsync();
        double recentPriceValue = 0;
        while (recentPriceReader.Read())
        {
            var value = recentPriceReader["value"].ToString();
            double.TryParse(value, out recentPriceValue);
        }

        await this.SqlConnection.CloseAsync();
        return recentPriceValue;
    }
}