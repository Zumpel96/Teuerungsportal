namespace Teuerungsportal.Services;

using Newtonsoft.Json;
using Teuerungsportal.Helpers;
using Teuerungsportal.Services.Interfaces;

public class ApiCategoryService : CategoryService
{
    private const string BaseUrl = "https://fun-teuerungsportal-prod-westeu-001.azurewebsites.net/api";
    private HttpClient Client { get; set; }

    public ApiCategoryService(HttpClient client)
    {
        this.Client = client;
    }

    /// <inheritdoc />
    public async Task<Category?> GetCategory(string categoryName)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/category/{categoryName}");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<Category>(responseBody);
        
        if (data == null)
        {
            return null;
        }

        var childCategoriesResponse = await this.Client.GetAsync($"{BaseUrl}/category/{data.Id}/categories");
        response.EnsureSuccessStatusCode();
        var childCategoriesResponseBody = await childCategoriesResponse.Content.ReadAsStringAsync();
        var childCategoriesData = JsonConvert.DeserializeObject<ICollection<Category>>(childCategoriesResponseBody);

        if (childCategoriesData == null)
        {
            return data;
        }
        
        data.SubCategories = childCategoriesData;
        
        return data;
    }

    /// <inheritdoc />
    public async Task<ICollection<Category>> GetCategoriesWithChildren()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Category>>(responseBody);
        if (data == null)
        {
            return new List<Category>();
        }

        var categoryDict = new Dictionary<string, Category>();
        foreach (var categoryData in data.OrderBy(c => c.RecursionId))
        {
            var category = new Category
                           {
                               Id = categoryData.Id,
                               Name = categoryData.Name,
                               RecursionId = categoryData.RecursionId,
                           };

            categoryDict[categoryData.RecursionId] = category;
            if (categoryData.RecursionId.IndexOf('.') <= 0)
            {
                continue;
            }

            var parentId = categoryData.RecursionId.Substring(0, categoryData.RecursionId.LastIndexOf('.'));
            var parentCategory = categoryDict[parentId];
            parentCategory.SubCategories.Add(category);
        }

        return categoryDict.Values.Where(category => category.RecursionId.All(ch => ch != '.')).ToList();
    }

    /// <inheritdoc />
    public async Task<ICollection<Category>> GetAllCategories()
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/categories");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ICollection<Category>>(responseBody);
        return data == null ? new List<Category>() : data.OrderBy(c => c.Name).ToList();
    }

    /// <inheritdoc />
    public async Task<int> GetNumberOfProducts(Guid categoryId)
    {
        var response = await this.Client.GetAsync($"{BaseUrl}/category/{categoryId}/products/number");

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<int>(responseBody);
        return data;
    }
}