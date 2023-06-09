namespace Api.Extractors.DM;

using System;
using System.Net.Http;
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
}

public class Price
{
    public double value { get; set; }

    public Guid productId { get; set; }
}

public class DmDataExtractor
{
    private const string DmStoreId = "d6635730-4afe-4d78-bb52-b702aa649f9d";

    private const string ProductIdCommand = "SELECT TOP(1) id FROM [dbo].[product] WHERE storeId=@storeId AND articleNumber=@articleNumber";

    private const string RecentPriceCommand = "SELECT TOP(1) value FROM [dbo].[price] WHERE productId=@productId ORDER BY timeStamp DESC";

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<Product> DbProducts { get; set; }

    private IAsyncCollector<Price> DbPrices { get; set; }

    public DmDataExtractor(string url, IAsyncCollector<Product> dbProducts, IAsyncCollector<Price> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = url;

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run()
    {
        // Initiate http call
        var response = await this.Client.GetAsync(this.Url);

        // Handle the http response
        var json = await response.Content.ReadAsStringAsync();
        dynamic responseData = JsonConvert.DeserializeObject(json);

        if (responseData == null)
        {
            return;
        }

        // Iterate over Data
        foreach (var hit in responseData["products"])
        {
            var data = hit;

            var articleNumber = $"{data["gtin"]}";

            // Create Product if it does not exist
            var existingProductId = await this.GetProduct(articleNumber);
            if (existingProductId == null)
            {
                var newProduct = new Product()
                                 {
                                     name = data["name"],
                                     articleNumber = articleNumber,
                                     url = data["relativeProductUrl"],
                                     brand = data["brandName"],
                                     storeId = new Guid(DmStoreId),
                                 };

                await this.DbProducts.AddAsync(newProduct);
                await this.DbProducts.FlushAsync();
            }

            // Fetch Product
            existingProductId = await this.GetProduct(articleNumber);
            if (existingProductId == null)
            {
                continue;
            }

            var parseSuccess = double.TryParse(data["price"]["value"].ToString(), out double newPriceValue);
            if (!parseSuccess)
            {
                continue;
            }

            // Check if Price has Changed
            var recentPriceValue = await this.GetRecentPrice((Guid)existingProductId);
            if (Math.Round(recentPriceValue, 2) == Math.Round(newPriceValue, 2))
            {
                continue;
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
    }

    private async Task<Guid?> GetProduct(string articleNumber)
    {
        var existsCommand = new SqlCommand(ProductIdCommand, this.SqlConnection);
        existsCommand.Parameters.AddWithValue("@storeId", DmStoreId);
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