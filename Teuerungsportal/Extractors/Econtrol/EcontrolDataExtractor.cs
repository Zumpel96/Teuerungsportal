namespace Api.Extractors.Econtrol;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using global::Extractors.General;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class EcontrolDataExtractor
{
    private const string Url = "https://www.e-control.at/o/rc-public-rest/rate-calculator/energy-type/POWER/rate";

    private readonly Guid EcontrolStoreId = new ("fe733623-f9c4-4952-9e57-d18d0492a91f");

    private readonly Guid CategoryId = new ("2c360bc9-641f-496a-ba2d-e42b290a9cb9");

    private HttpClient Client { get; set; }

    private string ZipCode { get; set; }

    private SqlConnection SqlConnection { get; set; }

    private IAsyncCollector<ProductDto> DbProducts { get; set; }

    private IAsyncCollector<PriceDto> DbPrices { get; set; }

    public EcontrolDataExtractor(string zipCode, IAsyncCollector<ProductDto> dbProducts, IAsyncCollector<PriceDto> dbPrices)
    {
        this.Client = new HttpClient();
        this.ZipCode = zipCode;

        var sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        this.SqlConnection = new SqlConnection(sqlConnectionString);

        this.DbPrices = dbPrices;
        this.DbProducts = dbProducts;
    }

    public async Task Run(ILogger log)
    {
        // Initiate http call
        var content = new StringContent(
                                        $"{{\"customerGroup\":\"HOME\",\"energyType\":\"POWER\",\"zipCode\":{this.ZipCode},\"gridOperatorId\":10601,\"gridAreaId\":651,\"moveHome\":false,\"includeSwitchingDiscounts\":true,\"firstMeterOptions\":{{\"standardConsumption\":3000,\"smartMeterRequestOptions\":{{\"smartMeterSearch\":false,\"loadProfileUpload\":false,\"loadProfileId\":null,\"loadProfileName\":null,\"consumptionType\":null,\"calculatedValues\":null,\"detailedValues\":null,\"lastUploadDate\":null}}}},\"comparisonOptions\":{{\"brandId\":4651,\"mainProductId\":null,\"additionalProductId\":null,\"feedInProductId\":null,\"mainProductAssociationId\":null,\"additionalProductAssociationId\":null,\"feedInProductAssociationId\":null,\"productName\":null,\"manualEntry\":false,\"mainBaseRate\":null,\"mainEnergyRate\":null}},\"membership\":null,\"requirements\":[],\"priceView\":\"CENT_PER_YEAR\",\"referencePeriod\":\"ONE_YEAR\",\"includeFeedInOptions\":false,\"searchPriceModel\":\"CLASSIC\"}}",
                                        Encoding.UTF8,
                                        "application/json");

        log.LogTrace("Executing HTTP Request");
        var response = await this.Client.PostAsync(Url, content);

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
        var existingData = await DataLoading.GetStoreProducts(log, this.EcontrolStoreId, this.SqlConnection);
        var upsertProducts = new List<ProductDto>();
        var insertPrices = new List<PriceDto>();

        var sum = 0f;
        var count = 0;

        // Iterate over Data
        log.LogTrace("Processing request response");
        foreach (var hit in responseData.ratedProducts)
        {
            log.LogTrace("Processing Entry");
            var price = hit["averageTotalPriceInCentKWh"];
            float.TryParse(price.ToString(), out float newVal);

            if (newVal == 0)
            {
                return;
            }

            newVal /= 100;
            sum += newVal;
            count++;
        }

        try
        {
            DataLoading.ProcessProductWithPrice(
                                                $"ECAPAT{this.ZipCode}",
                                                $"Durchschnitt Strom {this.ZipCode}",
                                                string.Empty,
                                                string.Empty,
                                                this.EcontrolStoreId,
                                                Math.Round(sum / count, 2),
                                                existingData,
                                                upsertProducts,
                                                insertPrices,
                                                log);
        }
        catch (Exception)
        {
            log.LogWarning("Could not Process Data!");
        }

        foreach (var product in upsertProducts)
        {
            product.categoryId = this.CategoryId;
        }

        await DataLoading.UpsertProducts(upsertProducts, this.DbProducts, log);
        await DataLoading.InsertPrices(insertPrices, this.DbPrices, log);
    }
}