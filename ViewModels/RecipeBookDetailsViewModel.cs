using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(BookId), "bookId")]
[QueryProperty(nameof(BookTitle), "bookTitle")]
public partial class RecipeBookDetailsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;

    [ObservableProperty]
    private int _bookId;

    [ObservableProperty]
    private string _bookTitle = string.Empty;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public RecipeBookDetailsViewModel(IApiService apiService, RecipeService recipeService, FavoritesService favoritesService)
    {
        _apiService = apiService;
        _recipeService = recipeService;
        _favoritesService = favoritesService;
    }

    partial void OnBookIdChanged(int value)
    {
        if (value > 0)
            _ = LoadRecipesAsync();
    }

    partial void OnBookTitleChanged(string value)
    {
        Title = value;
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        if (BookId <= 0) return;

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _apiService.GetRecipeBookRecipesAsync(BookId);
            Recipes.Clear();

            if (result.IsSuccess && result.Data != null)
            {
                foreach (var apiRecipe in result.Data.Recipes)
                {
                    var recipe = MapToRecipe(apiRecipe);
                    recipe.IsFavorite = await _favoritesService.IsFavoriteAsync(recipe.Id);
                    Recipes.Add(recipe);
                }
            }
            else
            {
                SetError("Could not load recipes.");
            }
        }
        catch
        {
            SetError("Failed to load recipes.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRecipeDetailAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={recipe.Id}");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Recipe recipe)
    {
        await _favoritesService.ToggleFavoriteAsync(recipe);
        recipe.IsFavorite = await _favoritesService.IsFavoriteAsync(recipe.Id);

        var index = Recipes.IndexOf(recipe);
        if (index >= 0)
            Recipes[index] = recipe;
    }

    private const string BaseUrl = "https://forkfeed.vercel.app";

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
        AuthorId = api.Author?.Id ?? 0,
        AuthorUsername = api.Author?.Username ?? string.Empty,
        AuthorProfileImageUrl = ResolveImageUrl(api.Author?.ProfileImageUrl),
    };

    private static string ResolveImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;
        return $"{BaseUrl}{(url.StartsWith('/') ? url : "/" + url)}";
    }

    private static string CapitalizeFirst(string value) =>
        string.IsNullOrEmpty(value) ? value
        : char.ToUpper(value[0]) + value[1..].ToLower();
}
