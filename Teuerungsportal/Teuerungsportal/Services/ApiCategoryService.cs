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
    public async Task<ICollection<Category>> GetCategories()
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
}