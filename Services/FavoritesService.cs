using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class FavoritesService
{
    private readonly List<Recipe> _favorites = new();

    public Task<List<Recipe>> GetFavoritesAsync()
    {
        return Task.FromResult(_favorites.ToList());
    }

    public Task<bool> IsFavoriteAsync(int recipeId)
    {
        return Task.FromResult(_favorites.Any(r => r.Id == recipeId));
    }

    public async Task ToggleFavoriteAsync(Recipe recipe)
    {
        await Task.Delay(100);

        var existing = _favorites.FirstOrDefault(r => r.Id == recipe.Id);
        if (existing != null)
        {
            _favorites.Remove(existing);
            recipe.IsFavorite = false;
        }
        else
        {
            recipe.IsFavorite = true;
            _favorites.Add(recipe);
        }
    }
}
