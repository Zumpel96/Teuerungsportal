namespace Teuerungsportal.Services;

using Blazored.LocalStorage;
using Teuerungsportal.Services.Interfaces;

public class LocalFavoriteService : FavoriteService
{
    public LocalFavoriteService(ILocalStorageService localStorageService)
    {
        this.LocalStorageService = localStorageService;
    }
    
    private ILocalStorageService LocalStorageService { get; set; }

    /// <inheritdoc />
    public async Task FavoriteProduct(Guid productId)
    {
        var allFavorites = await this.GetFavorites();
        if (allFavorites.Contains(productId))
        {
            return;
        }
        
        allFavorites.Add(productId);
        await this.LocalStorageService.SetItemAsync("favorites", allFavorites);
    }

    /// <inheritdoc />
    public async Task UnFavoriteProduct(Guid productId)
    {
        var allFavorites = await this.GetFavorites();
        if (!allFavorites.Contains(productId))
        {
            return;
        }

        allFavorites.Remove(productId);
        await this.LocalStorageService.SetItemAsync("favorites", allFavorites);
    }

    /// <inheritdoc />
    public async Task<ICollection<Guid>> GetFavorites()
    {
        var favorites = await this.LocalStorageService.GetItemAsync<ICollection<Guid>>("favorites");
        return favorites ?? new List<Guid>();
    }
}