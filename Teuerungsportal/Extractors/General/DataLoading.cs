namespace Extractors.General;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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
}