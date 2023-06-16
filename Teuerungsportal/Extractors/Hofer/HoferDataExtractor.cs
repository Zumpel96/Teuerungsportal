namespace Api.Extractors.Hofer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
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

public class HoferDataExtractor
{
    private const string HoferStoreId = "b8aa3d69-3f74-4ce4-acae-ac147058c483";

    private const string ProductIdCommand = "SELECT TOP(1) id FROM [dbo].[product] WHERE storeId=@storeId AND articleNumber=@articleNumber";

    private const string RecentPriceCommand = "SELECT TOP(1) value FROM [dbo].[price] WHERE productId=@productId ORDER BY timeStamp DESC";

    private HttpClient Client { get; set; }

    private string Category { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<Product> DbProducts { get; set; }

    private IAsyncCollector<Price> DbPrices { get; set; }

    public HoferDataExtractor(string category, IAsyncCollector<Product> dbProducts, IAsyncCollector<Price> dbPrices)
    {
        this.Client = new HttpClient();
        this.Client.DefaultRequestHeaders.Add("Accept", "application/json");
        this.Category = category;

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run(ILogger log)
    {
        // Get token
        var tokenBody = new Dictionary<string, string>()
                        {
                            { "OwnWebshopProviderCode", "" },
                            { "SetUserSelectedShopsOnFirstSiteLoad", "true" },
                            { "RedirectToDashboardNeeded", "false" },
                            { "ShopsSelectedForRoot", "hofer" },
                            { "BrandProviderSelectedForRoot", "null" },
                            { "UserSelectedShops", "[]" }
                        };
        var tokenContent = new FormUrlEncodedContent(tokenBody);
        tokenContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var tokenResponse = await this.Client.PostAsync("https://shopservice.roksh.at/session/configure", tokenContent);
        if (!tokenResponse.Headers.TryGetValues("JWT-Auth", out var tokenValues))
        {
            return;
        }

        var token = tokenValues.First();
        this.Client.DefaultRequestHeaders.Add("Bearer", token);

        // Get product pages
        var pagesBody = new Dictionary<string, string>()
                        {
                            { "CategoryProgId", this.Category },
                            { "Page", "9999999" },
                        };

        var pagesRequest = new HttpRequestMessage(HttpMethod.Post, "https://shopservice.roksh.at/productlist/GetProductList");
        pagesRequest.Headers.Add("Bearer", token);
        
        var pagesContent = new StringContent(JsonConvert.SerializeObject(pagesBody), null, "application/json");
        pagesRequest.Content = pagesContent;
        
        var pagesResponse = await this.Client.SendAsync(pagesRequest);
        var pagesResponseJson = await pagesResponse.Content.ReadAsStringAsync();

        dynamic pagesResponseData = JsonConvert.DeserializeObject(pagesResponseJson);
        if (pagesResponseData == null)
        {
            return;
        }
        
        if (!int.TryParse(pagesResponseData["TotalPages"].ToString(), out int numberOfPages))
        {
            return;
        }

        for (var i = 0; i < numberOfPages; i++)
        {
            // Get products
            var productsBody = new Dictionary<string, string>()
                               {
                                   { "CategoryProgId", this.Category },
                                   { "Page", $"{i}" },
                               };

            var productsRequest = new HttpRequestMessage(HttpMethod.Post, "https://shopservice.roksh.at/productlist/GetProductList");
            productsRequest.Headers.Add("Bearer", token);

            var productsContent = new StringContent(JsonConvert.SerializeObject(productsBody), null, "application/json");
            productsRequest.Content = productsContent;

            var productsResponse = await this.Client.SendAsync(productsRequest);
            var productsResponseJson = await productsResponse.Content.ReadAsStringAsync();
            
            dynamic responseData = JsonConvert.DeserializeObject(productsResponseJson);
            if (responseData == null)
            {
                return;
            }

            // Iterate over Data
            foreach (var hit in responseData["ProductList"])
            {
                var data = hit;
                var articleNumber = $"{data["ProductID"]}";

                // Create Product if it does not exist
                var existingProductId = await this.GetProduct(articleNumber);
                if (existingProductId == null)
                {
                    var newProduct = new Product()
                                     {
                                         id = Guid.NewGuid(),
                                         name = data["ProductName"],
                                         articleNumber = articleNumber,
                                         url = string.Empty,
                                         brand = data["Brand"],
                                         storeId = new Guid(HoferStoreId),
                                     };

                    await this.DbProducts.AddAsync(newProduct);
                    await this.DbProducts.FlushAsync();
                }
                else
                {
                    var existingProduct = new Product()
                                          {
                                              id = (Guid)existingProductId,
                                              name = data["ProductName"],
                                              articleNumber = articleNumber,
                                              url = string.Empty,
                                              brand = data["Brand"],
                                              storeId = new Guid(HoferStoreId),
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

                var parseSuccess = double.TryParse(data["Price"].ToString(), out double newPriceValue);
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
    }

    private async Task<Guid?> GetProduct(string articleNumber)
    {
        var existsCommand = new SqlCommand(ProductIdCommand, this.SqlConnection);
        existsCommand.Parameters.AddWithValue("@storeId", HoferStoreId);
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