namespace Api.Extractors.Penny;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class PennyDataExtractor
{
    private readonly Guid PennyStoreId = new ("420207b4-519d-4919-ab45-1e55b769aefe");

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public PennyDataExtractor(int page, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = $"https://www.penny.at/api/products?page={page}&pageSize=500";

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run(ILogger log)
    {
        // Initiate http call
        log.LogTrace("Executing HTTP Request");
        var response = await this.Client.GetAsync(this.Url);
        if (!response.IsSuccessStatusCode)
        {
            log.LogError("Request failed");
            return;
        }

        // Handle the http response
        var json = await response.Content.ReadAsStringAsync();
        dynamic responseData = JsonConvert.DeserializeObject(json);
        if (responseData == null)
        {
            log.LogError("Response Body was empty!");
            return;
        }

        // Read existing products
        log.LogTrace("Loading existing Data");
        var existingData = await DataLoading.GetStoreProducts(log, this.PennyStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();
        
        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var data in responseData["results"])
        {
            log.LogTrace("Processing Entry");
            var articleNumber = $"{data["sku"]}";

            if (!double.TryParse(data["price"]["regular"]["value"].ToString(), out double newPriceValue))
            {
                continue;
            }

            newPriceValue /= 100;
            newPriceValue = Math.Round(newPriceValue, 2);

            // Check if product exists
            if (existingData.TryGetValue(articleNumber, out var value))
            {
                log.LogTrace("Existing Product");
                var existingProduct = value.Product;
                var newProduct = new ProductDto()
                                 {
                                     id = existingProduct.id,
                                     name = data["name"],
                                     articleNumber = articleNumber,
                                     url = data["slug"],
                                     brand = string.Empty,
                                     storeId = this.PennyStoreId,
                                     categoryId = existingProduct.categoryId,
                                 };

                if (!existingProduct.Equals(newProduct))
                {
                    log.LogInformation("Updating Product");
                    upsertProducts.Add(newProduct);
                }

                var currentPrice = value.Price;
                if (currentPrice != null && Math.Round((double)currentPrice, 2) != newPriceValue)
                {
                    log.LogInformation("Adding Price");
                    var newPrice = new PriceDto()
                                   {
                                       value = newPriceValue,
                                       productId = existingProduct.id,
                                   };

                    insertPrices.Add(newPrice);
                }
            }
            else
            {
                log.LogInformation("New Product");

                var newProduct = new ProductDto()
                                 {
                                     id = Guid.NewGuid(),
                                     name = data["name"],
                                     articleNumber = articleNumber,
                                     url = data["slug"],
                                     brand = string.Empty,
                                     storeId = this.PennyStoreId,
                                     categoryId = null,
                                 };

                var newPrice = new PriceDto()
                               {
                                   value = newPriceValue,
                                   productId = newProduct.id,
                               };

                existingData.Add(articleNumber, (newProduct, newPriceValue));

                upsertProducts.Add(newProduct);
                insertPrices.Add(newPrice);
            }
        }

        log.LogInformation($"Upserting {upsertProducts.Count} Products");
        foreach (var product in upsertProducts)
        {
            await this.DbProducts.AddAsync(product);
        }

        await this.DbProducts.FlushAsync();

        log.LogInformation($"Inserting {insertPrices.Count} Prices");
        foreach (var price in insertPrices)
        {
            await this.DbPrices.AddAsync(price);
        }

        await this.DbPrices.FlushAsync();
    }
}