namespace Api.Extractors.DM;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class DmDataExtractor
{
    private readonly Guid DmStoreId = new ("d6635730-4afe-4d78-bb52-b702aa649f9d");

    private string Url { get; set; }

    private HttpClient Client { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public DmDataExtractor(string category, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = $"https://product-search.services.dmtech.com/at/search/crawl?pageSize=10000&allCategories.id={category}";

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
        var existingData = await DataLoading.GetStoreProducts(log, this.DmStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();
        
        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var data in responseData["products"])
        {
            log.LogTrace("Processing Entry");
            var articleNumber = $"{data["gtin"]}";

            if (!double.TryParse(data["price"]["value"].ToString(), out double newPriceValue))
            {
                continue;
            }

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
                                     url = data["relativeProductUrl"],
                                     brand = data["brandName"],
                                     storeId = this.DmStoreId,
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
                                     url = data["relativeProductUrl"],
                                     brand = data["brandName"],
                                     storeId = this.DmStoreId,
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