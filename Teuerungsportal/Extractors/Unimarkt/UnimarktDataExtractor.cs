namespace Api.Extractors.Unimarkt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Extractors.General;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public class UnimarktDataExtractor
{
    private readonly Guid UnimarktStoreId = new ("1f8b6fd6-a37e-47cd-a509-2a910a3396b3");

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public UnimarktDataExtractor(string category, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Url = $"https://shop.unimarkt.at/{category}";

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run(ILogger log)
    {
        // Initiate http call
        log.LogTrace("Executing HTTP Request");
        var client = new HtmlWeb();
        var response = client.Load(this.Url);
        if (response == null)
        {
            log.LogError("Request failed");
            return;
        }

        // Fetch Products from DOM
        var products = response.DocumentNode.SelectNodes("//div[@ class='produktContainer']");

        // Read existing products
        log.LogTrace("Loading existing Data");
        var existingData = await DataLoading.GetStoreProducts(log, this.UnimarktStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();
        
        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var data in products)
        {
            log.LogTrace("Processing Entry");
            var articleNumber = $"{data.Attributes.FirstOrDefault(a => a.Name == "data-articleid")?.Value}";
            var name = $"{data.Descendants("span").FirstOrDefault(c => c.Attributes["class"].Value.Contains("name"))?.InnerText}";
            var brand = $"{data.Attributes.FirstOrDefault(a => a.Name == "data-marke")?.Value}";
            var price = $"{data.Attributes.FirstOrDefault(a => a.Name == "data-price")?.Value}";
            var url = $"/{data.Attributes.FirstOrDefault(a => a.Name == "data-name")?.Value}-{articleNumber}";

            if (!double.TryParse(price, out double newPriceValue))
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
                                     name = name,
                                     articleNumber = articleNumber,
                                     url = url,
                                     brand = brand,
                                     storeId = this.UnimarktStoreId,
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
                                     name = name,
                                     articleNumber = articleNumber,
                                     url = url,
                                     brand = brand,
                                     storeId = this.UnimarktStoreId,
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