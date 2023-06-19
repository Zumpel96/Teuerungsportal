namespace Api.Extractors.Lidl;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class LidlDataExtractor
{
    private readonly Guid LidlStoreId = new ("bdf1f1ad-84d5-4cfa-a313-c18e8899cb2f");

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public LidlDataExtractor(IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url = "https://www.lidl.at/p/api/gridboxes/AT/de/?max=100000";

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
        var existingData = await DataLoading.GetStoreProducts(log, this.LidlStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();
        
        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var data in responseData)
        {
            try
            {
                DataLoading.ProcessProductWithPrice(
                                                    data["erpNumber"],
                                                    data["fullTitle"],
                                                    data["canonicalUrl"],
                                                    data["keyfacts"]["supplementalDescription"],
                                                    this.LidlStoreId,
                                                    data["price"]["price"],
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