namespace Api.Extractors.Billa;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class BillaDataExtractor
{
    private readonly Guid BillaStoreId = new ("6cce1289-453c-4c02-83a3-e73e61639d24");

    private string Url { get; set; }

    private HttpClient Client { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public BillaDataExtractor(string category, int page, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = $"https://shop.billa.at/api/categories/{category}/products?page={page}&pageSize=500";

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
        var existingData = await DataLoading.GetStoreProducts(log, this.BillaStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();

        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var data in responseData["results"])
        {
            try
            {
                DataLoading.ProcessProductWithPrice(
                                                    data["sku"],
                                                    data["name"],
                                                    data["slug"],
                                                    data["brand"]["name"],
                                                    this.BillaStoreId,
                                                    (float)data["price"]["regular"]["value"] / 100f,
                                                    existingData,
                                                    upsertProducts,
                                                    insertPrices,
                                                    log);
            }
            catch (Exception)
            {
                log.LogWarning("Could not Process Data!");
            }
        }

        await DataLoading.UpsertProducts(upsertProducts, this.DbProducts, log);
        await DataLoading.InsertPrices(insertPrices, this.DbPrices, log);
    }
}