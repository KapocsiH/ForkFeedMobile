using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class FavoritesService
{
    private readonly IApiService _api;
    private readonly HashSet<int> _favoriteIds = new();
    private bool _cacheLoaded;

    private const string BaseUrl = "https://forkfeed.vercel.app";

    public FavoritesService(IApiService api)
    {
        _api = api;
    }

    public async Task<List<Recipe>> GetFavoritesAsync()
    {
        var result = await _api.GetMyFavoritesAsync(1, 100);

        if (!result.IsSuccess || result.Data == null)
            return new List<Recipe>();

        var apiRecipes = result.Data.AllRecipes;

        // The favorites endpoint may return incomplete recipe objects (e.g. missing
        // preparation_time). Fetch the full recipe list and merge any missing data.
        var needsSupplement = apiRecipes.Any(r => r.PreparationTime == 0);
        if (needsSupplement)
        {
            var fullResult = await _api.GetRecipesAsync(1, 100);
            if (fullResult.IsSuccess && fullResult.Data != null)
            {
                var lookup = fullResult.Data.Recipes.ToDictionary(r => r.Id);
                for (var i = 0; i < apiRecipes.Count; i++)
                {
                    if (lookup.TryGetValue(apiRecipes[i].Id, out var full))
                    {
                        if (apiRecipes[i].PreparationTime == 0)
                            apiRecipes[i].PreparationTime = full.PreparationTime;
                        if (apiRecipes[i].AverageRating == 0)
                            apiRecipes[i].AverageRating = full.AverageRating;
                        if (string.IsNullOrWhiteSpace(apiRecipes[i].ImageUrl))
                            apiRecipes[i].ImageUrl = full.ImageUrl;
                        if (string.IsNullOrWhiteSpace(apiRecipes[i].Description))
                            apiRecipes[i].Description = full.Description;
                    }
                }
            }
        }

        var recipes = apiRecipes.Select(MapToRecipe).ToList();

        // Sync the local cache with what the server returned
        _favoriteIds.Clear();
        foreach (var r in recipes)
        {
            r.IsFavorite = true;
            _favoriteIds.Add(r.Id);
        }
        _cacheLoaded = true;

        return recipes;
    }

    public Task<bool> IsFavoriteAsync(int recipeId)
    {
        return Task.FromResult(_favoriteIds.Contains(recipeId));
    }

    /// <summary>
    /// Toggles the favorite state on the backend and updates the local cache.
    /// Returns true if the API call succeeded.
    /// </summary>
    public async Task<bool> ToggleFavoriteAsync(Recipe recipe)
    {
        var wasFavorite = _favoriteIds.Contains(recipe.Id);

        try
        {
            if (wasFavorite)
            {
                var result = await _api.RemoveRecipeFavoriteAsync(recipe.Id);
                if (!result.IsSuccess)
                    return false;

                recipe.IsFavorite = false;
                _favoriteIds.Remove(recipe.Id);
            }
            else
            {
                var result = await _api.AddRecipeFavoriteAsync(recipe.Id);
                if (!result.IsSuccess)
                    return false;

                recipe.IsFavorite = true;
                _favoriteIds.Add(recipe.Id);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static Recipe MapToRecipe(ApiRecipe api) => new()
    {
        Id = api.Id,
        Title = api.Title,
        Description = api.Description ?? string.Empty,
        ImageUrl = ResolveImageUrl(api.ImageUrl),
        Difficulty = CapitalizeFirst(api.Difficulty),
        TimeMinutes = api.PreparationTime,
        Rating = api.AverageRating,
        CreatedAt = api.CreatedAt,
        IsFavorite = true
    };

    private static string ResolveImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return $"{BaseUrl}{(url.StartsWith('/') ? url : "/" + url)}";
    }

    private static string CapitalizeFirst(string value) =>
        string.IsNullOrEmpty(value) ? value
        : char.ToUpper(value[0]) + value[1..].ToLower();
}
