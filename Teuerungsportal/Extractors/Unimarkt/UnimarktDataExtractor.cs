namespace Api.Extractors.Unimarkt;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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

public class UnimarktDataExtractor
{
    private const string UnimarktStoreId = "1f8b6fd6-a37e-47cd-a509-2a910a3396b3";

    private const string ProductIdCommand = "SELECT TOP(1) id FROM [dbo].[product] WHERE storeId=@storeId AND articleNumber=@articleNumber";

    private const string RecentPriceCommand = "SELECT TOP(1) value FROM [dbo].[price] WHERE productId=@productId ORDER BY timeStamp DESC";

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<Product> DbProducts { get; set; }

    private IAsyncCollector<Price> DbPrices { get; set; }

    public UnimarktDataExtractor(string category, IAsyncCollector<Product> dbProducts, IAsyncCollector<Price> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = $"https://shop.unimarkt.at/{category}";

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run()
    {
        // Get http page
        var client = new HtmlWeb();

        // Handle the http response
        var response = client.Load(this.Url);
        if (response == null)
        {
            return;
        }

        var products = response.DocumentNode.SelectNodes("//div[@ class='produktContainer']");

        // Iterate over Data
        foreach (var product in products)
        {
            var articleNumber = $"{product.Attributes.FirstOrDefault(a => a.Name == "data-articleid")?.Value}";
            var name = $"{product.Descendants("span").FirstOrDefault(c => c.Attributes["class"].Value.Contains("name"))?.InnerText}";
            var brand = $"{product.Attributes.FirstOrDefault(a => a.Name == "data-marke")?.Value}";
            var price = $"{product.Attributes.FirstOrDefault(a => a.Name == "data-price")?.Value}";
            var url = $"/{product.Attributes.FirstOrDefault(a => a.Name == "data-name")?.Value}-{articleNumber}";

            // Create Product if it does not exist
            var existingProductId = await this.GetProduct(articleNumber);
            if (existingProductId == null)
            {
                var newProduct = new Product()
                                 {
                                     id = Guid.NewGuid(),
                                     name = name,
                                     articleNumber = articleNumber,
                                     url = url,
                                     brand = brand,
                                     storeId = new Guid(UnimarktStoreId),
                                 };

                await this.DbProducts.AddAsync(newProduct);
                await this.DbProducts.FlushAsync();
            }
            else
            {
                var existingProduct = new Product()
                                      {
                                          id = (Guid)existingProductId,
                                          name = name,
                                          articleNumber = articleNumber,
                                          url = url,
                                          brand = brand,
                                          storeId = new Guid(UnimarktStoreId),
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

            var parseSuccess = double.TryParse(price, out double newPriceValue);
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
        existsCommand.Parameters.AddWithValue("@storeId", UnimarktStoreId);
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