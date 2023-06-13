namespace Api.Extractors.MPreis;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

public class Product
{
    public Guid id { get; set; }

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

public class MPreisDataExtractor
{
    private const string MPreisStoreId = "101d0286-9c9c-448b-86bc-66e108303126";

    private const string ProductIdCommand = "SELECT TOP(1) id FROM [dbo].[product] WHERE storeId=@storeId AND articleNumber=@articleNumber";

    private const string RecentPriceCommand = "SELECT TOP(1) value FROM [dbo].[price] WHERE productId=@productId ORDER BY timeStamp DESC";

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<Product> DbProducts { get; set; }

    private IAsyncCollector<Price> DbPrices { get; set; }

    public MPreisDataExtractor(IAsyncCollector<Product> dbProducts, IAsyncCollector<Price> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = "https://ax2ixv4hll-dsn.algolia.net/1/indexes/prod_mpreis_8450/browse?X-Algolia-API-Key=NmJlMTI0NjY1NGU4MDUwYTRlMmYzYWFjOWFlY2U4MGFkNGZjMDY2NmNjNjQzNWY3OWJlNDY4OTY0ZjEwOTEwYWZpbHRlcnM9cHVibGlzaGVk&X-Algolia-Application-Id=AX2IXV4HLL&X-Algolia-Agent=Vue.js";

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
        foreach (var hit in responseData["hits"])
        {
            var data = hit;

            var articleNumber = $"{data["code"]}";

            // Create Product if it does not exist
            var existingProductId = await this.GetProduct(articleNumber);
            if (existingProductId == null)
            {
                var newProduct = new Product()
                                 {
                                     id = Guid.NewGuid(),
                                     name = data["name"][0],
                                     articleNumber = articleNumber,
                                     url = articleNumber,
                                     brand = data["mixins"]["markantAttributes"]["data"][0]["description"]["brandName"],
                                     storeId = new Guid(MPreisStoreId),
                                 };

                await this.DbProducts.AddAsync(newProduct);
                await this.DbProducts.FlushAsync();
            }
            else
            {
                var existingProduct = new Product()
                                      {
                                          id = (Guid)existingProductId,
                                          name = data["name"][0],
                                          articleNumber = articleNumber,
                                          url = articleNumber,
                                          brand = data["mixins"]["markantAttributes"]["data"][0]["description"]["brandName"],
                                          storeId = new Guid(MPreisStoreId),
                                      };

                await this.DbProducts.AddAsync(existingProduct);
                await this.DbProducts.FlushAsync();
            }

            // Fetch Product
            existingProductId = await this.GetProduct(articleNumber);
            if (existingProductId == null)
            {
                continue;
            }

            var parseSuccess = double.TryParse(data["prices"][0]["effectiveAmount"].ToString(), out double newPriceValue);
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
        existsCommand.Parameters.AddWithValue("@storeId", MPreisStoreId);
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