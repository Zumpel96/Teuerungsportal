namespace Api.Extractors.Econtrol;

using System;
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

        var articleNumber = $"ECAPAT{this.ZipCode}";
        var newPriceValue = Math.Round(sum / count, 2);

        // Check if product exists
        if (existingData.TryGetValue(articleNumber, out var value))
        {
            log.LogTrace("Existing Product");
            var existingProduct = value.Product;
            var newProduct = new ProductDto()
                             {
                                 id = existingProduct.id,
                                 name = $"Durchschnitt Strom {this.ZipCode}",
                                 articleNumber = articleNumber,
                                 url = string.Empty,
                                 brand = string.Empty,
                                 storeId = this.EcontrolStoreId,
                                 categoryId = this.CategoryId,
                             };

            if (!existingProduct.Equals(newProduct))
            {
                log.LogInformation("Updating Product");
                await this.DbProducts.AddAsync(newProduct);
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

                await this.DbPrices.AddAsync(newPrice);
            }
        }
        else
        {
            log.LogInformation("New Product");

            var newProduct = new ProductDto()
                             {
                                 id = Guid.NewGuid(),
                                 name = $"Durchschnitt Strom {this.ZipCode}",
                                 articleNumber = articleNumber,
                                 url = string.Empty,
                                 brand = string.Empty,
                                 storeId = this.EcontrolStoreId,
                                 categoryId = this.CategoryId,
                             };

            var newPrice = new PriceDto()
                           {
                               value = newPriceValue,
                               productId = newProduct.id,
                           };

            existingData.Add(articleNumber, (newProduct, newPriceValue));
            
            await this.DbProducts.AddAsync(newProduct);
            await this.DbPrices.AddAsync(newPrice);
        }

        await this.DbPrices.FlushAsync();
    }
}