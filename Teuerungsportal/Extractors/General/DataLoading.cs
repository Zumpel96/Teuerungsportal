#nullable enable
namespace Extractors.General;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class DataLoading
{
    private const string Command = @"
        SELECT 
          [p].[id] AS [id], 
          [p].[name] AS [name], 
          [p].[articleNumber] AS [articleNumber], 
          [p].[url] AS [url], 
          [p].[brand] AS [brand], 
          [p].[storeId] AS [storeId], 
          [p].[categoryId] AS [categoryId], 
          [pr].[value] AS [value] 
        FROM 
          [dbo].[product] [p] 
          JOIN (
            SELECT 
              [productId], 
              MAX([timestamp]) AS [maxTimestamp] 
            FROM 
              [dbo].[price] 
            GROUP BY 
              [productId]
          ) [maxPr] ON [p].[id] = [maxPr].[productId] 
          JOIN [dbo].[price] [pr] ON [pr].[productId] = [p].[id] 
          AND [pr].[timestamp] = [maxPr].[maxTimestamp] 
        WHERE 
          [p].[storeId] = @storeId";

    public static async Task<IDictionary<string, (ProductDto Product, double? Price)>> GetStoreProducts(
        ILogger log,
        Guid storeId,
        SqlConnection connection)
    {
        var command = new SqlCommand(Command, connection);
        command.Parameters.AddWithValue("@storeId", storeId);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        var returnDictionary = new Dictionary<string, (ProductDto Product, double? Price)>();
        while (reader.Read())
        {
            var product = new ProductDto()
                          {
                              id = new Guid(reader["id"]?.ToString() ?? string.Empty),
                              name = reader["name"]?.ToString() ?? string.Empty,
                              brand = reader["brand"]?.ToString() ?? string.Empty,
                              articleNumber = reader["articleNumber"]?.ToString() ?? string.Empty,
                              url = reader["url"]?.ToString() ?? string.Empty,
                              storeId = new Guid(reader["storeId"]?.ToString() ?? string.Empty),
                              categoryId = string.IsNullOrEmpty(reader["categoryId"]?.ToString())
                                           ? null
                                           : new Guid(reader["categoryId"]?.ToString() ?? string.Empty),
                          };

            if (double.TryParse(reader["value"].ToString(), out var value))
            {
                returnDictionary.Add(product.articleNumber, (product, value));
            }
        }

        await connection.CloseAsync();
        return returnDictionary;
    }

    public static async Task UpsertProducts(ICollection<ProductDto> upsertProducts, IAsyncCollector<ProductDto> dbProducts, ILogger log)
    {
        log.LogInformation($"Upserting {upsertProducts.Count} Products");
        foreach (var product in upsertProducts)
        {
            try
            {
                log.LogInformation($"Upserting {product.id}");
                await dbProducts.AddAsync(product);
                await dbProducts.FlushAsync();
            }
            catch (Exception)
            {
                log.LogWarning($"Skipped product: {JsonConvert.SerializeObject(product)}");
            }
        }
    }

    public static async Task InsertPrices(ICollection<PriceDto> insertPrices, IAsyncCollector<PriceDto> dbPrices, ILogger log)
    {
        log.LogInformation($"Inserting {insertPrices.Count} Prices");
        foreach (var price in insertPrices)
        {
            try
            {
                log.LogInformation($"Inserting {price.value} for product {price.productId}");
                await dbPrices.AddAsync(price);
                await dbPrices.FlushAsync();
            }
            catch (Exception)
            {
                log.LogWarning($"Skipped price: {JsonConvert.SerializeObject(price)}");
            }
        }
    }

    public static void ProcessProductWithPrice(
        dynamic articleNumberData,
        dynamic nameData,
        dynamic urlData,
        dynamic brandData,
        Guid storeId,
        dynamic priceData,
        IDictionary<string, (ProductDto Product, double? Price)> existingData,
        ICollection<ProductDto> upsertProducts,
        ICollection<PriceDto> insertPrices,
        ILogger log,
        double priceModificator = 1)
    {
        var articleNumber = $"{articleNumberData}";
        var name = $"{nameData}";
        var url = $"{urlData}";
        var brand = $"{brandData}";
        var price = $"{priceData}";

        log.LogInformation($"Processing Entry: {articleNumber} | {name} | {url} | {brand}");
        if (string.IsNullOrEmpty(articleNumber) || string.IsNullOrEmpty(name))
        {
            log.LogWarning("Missing field(s)!");
            return;
        }

        if (!double.TryParse(price, out var priceValue))
        {
            log.LogWarning("Price could not be parsed!");
            return;
        }

        priceValue = Math.Round(priceValue / priceModificator, 2);
        log.LogInformation($"With Price: {priceValue}");

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
                                 storeId = storeId,
                                 categoryId = existingProduct.categoryId,
                             };

            if (!existingProduct.Equals(newProduct))
            {
                log.LogInformation("Updating Product");
                log.LogInformation($"Id: {existingProduct.id} | {newProduct.id}");
                log.LogInformation($"Name: {existingProduct.name} | {newProduct.name}");
                log.LogInformation($"Article Number: {existingProduct.articleNumber} | {newProduct.articleNumber}");
                log.LogInformation($"Url: {existingProduct.url} | {newProduct.url}");
                log.LogInformation($"Brand: {existingProduct.brand} | {newProduct.brand}");
                log.LogInformation($"StoreId: {existingProduct.storeId} | {newProduct.storeId}");
                log.LogInformation($"CategoryId: {existingProduct.categoryId} | {newProduct.categoryId}");
                upsertProducts.Add(newProduct);
            }

            var currentPrice = value.Price;
            if (currentPrice != null && !(Math.Abs((double)currentPrice - priceValue) >= 0.01))
            {
                log.LogTrace("No Price Change");
                return;
            }

            log.LogInformation("Adding Price");
            var newPrice = new PriceDto()
                           {
                               value = priceValue,
                               productId = existingProduct.id,
                               timestamp = DateTime.Now,
                           };

            insertPrices.Add(newPrice);
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
                                 storeId = storeId,
                                 categoryId = null,
                             };

            var newPrice = new PriceDto()
                           {
                               value = priceValue,
                               productId = newProduct.id,
                               timestamp = DateTime.Now,
                           };

            existingData.Add(articleNumber, (newProduct, priceValue));

            upsertProducts.Add(newProduct);
            insertPrices.Add(newPrice);
        }
    }
}