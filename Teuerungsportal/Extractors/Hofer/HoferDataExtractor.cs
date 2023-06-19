namespace Api.Extractors.Hofer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class HoferDataExtractor
{
    private readonly Guid HoferStoreId = new ("b8aa3d69-3f74-4ce4-acae-ac147058c483");

    private HttpClient Client { get; set; }

    private string Category { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public HoferDataExtractor(string category, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
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

        log.LogTrace("Fetching Token");
        var tokenResponse = await this.Client.PostAsync("https://shopservice.roksh.at/session/configure", tokenContent);

        if (!tokenResponse.Headers.TryGetValues("JWT-Auth", out var tokenValues))
        {
            log.LogError("Token not found in Header");
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

        log.LogTrace("Fetching Page Data");
        var pagesResponse = await this.Client.SendAsync(pagesRequest);

        var pagesResponseJson = await pagesResponse.Content.ReadAsStringAsync();
        dynamic pagesResponseData = JsonConvert.DeserializeObject(pagesResponseJson);

        if (pagesResponseData == null)
        {
            log.LogError("Could not fetch page data");
            return;
        }

        if (!int.TryParse(pagesResponseData["TotalPages"].ToString(), out int numberOfPages))
        {
            log.LogError("Could not load TotalPages from Body");
            return;
        }

        // Read existing products
        log.LogTrace("Loading existing Data");
        var existingData = await DataLoading.GetStoreProducts(log, this.HoferStoreId, this.SqlConnection);

        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();

        log.LogTrace($"{numberOfPages} Pages");
        for (var i = 0; i < numberOfPages; i++)
        {
            log.LogTrace($"Page {i}");

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

            log.LogTrace("Loading page data");
            var productsResponse = await this.Client.SendAsync(productsRequest);

            var productsResponseJson = await productsResponse.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(productsResponseJson);
            if (responseData == null)
            {
                log.LogError("Response Body was empty!");
                return;
            }

            // Iterate over Data
            log.LogTrace("Processing request response");
            foreach (var data in responseData["ProductList"])
            {
                try
                {
                    DataLoading.ProcessProductWithPrice(
                                                        data["ProductID"],
                                                        data["ProductName"],
                                                        string.Empty,
                                                        data["Brand"],
                                                        this.HoferStoreId,
                                                        data["Price"],
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
        }

        await DataLoading.UpsertProducts(upsertProducts, this.DbProducts, log);
        await DataLoading.InsertPrices(insertPrices, this.DbPrices, log);
    }
}