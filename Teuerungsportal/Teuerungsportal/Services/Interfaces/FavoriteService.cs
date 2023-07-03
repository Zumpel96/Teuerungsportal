namespace Teuerungsportal.Services.Interfaces;

public interface FavoriteService
{
    public Task FavoriteProduct(Guid productId);
    
    public Task UnFavoriteProduct(Guid productId);
    
    public Task<ICollection<Guid>> GetFavorites();
}