namespace Api.Extractors.Spar;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class SparDataExtractor
{
    private readonly Guid SparStoreId = new ("216c4a42-ef9c-4241-ab77-e41ae3ee1c34");

    private HttpClient Client { get; set; }

    private string Url { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public SparDataExtractor(string category, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.Url =
        $"https://search-spar.spar-ics.com/fact-finder/rest/v4/search/products_lmos_at?query=*&q=*&page=1&hitsPerPage=1000000&filter=category-path:{category}";

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
        var existingData = await DataLoading.GetStoreProducts(log, this.SparStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();

        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var hit in responseData["hits"])
        {
            var data = hit["masterValues"];
            if (data["item-type"] != "Product")
            {
                continue;
            }

            try
            {
                DataLoading.ProcessProductWithPrice(
                                                    data["product-number"],
                                                    data["short-description"],
                                                    data["url"],
                                                    data["brand"][0],
                                                    this.SparStoreId,
                                                    data["price"],
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