using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;

    [ObservableProperty]
    private int _recipeId;

    [ObservableProperty]
    private Recipe? _recipe;

    [ObservableProperty]
    private int _userRating;

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();

    public RecipeDetailViewModel(RecipeService recipeService, FavoritesService favoritesService)
    {
        _recipeService = recipeService;
        _favoritesService = favoritesService;
    }

    partial void OnRecipeIdChanged(int value)
    {
        _ = LoadRecipeAsync();
    }

    [RelayCommand]
    private async Task LoadRecipeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();

            var recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (recipe == null)
            {
                SetError("Recipe not found.");
                return;
            }

            recipe.IsFavorite = await _favoritesService.IsFavoriteAsync(recipe.Id);
            Recipe = recipe;
            Title = recipe.Title;
            UserRating = (int)Math.Round(recipe.Rating);

            Ingredients.Clear();
            foreach (var i in recipe.Ingredients)
                Ingredients.Add(new Ingredient { Name = i.Name, Quantity = i.Quantity });

            Steps.Clear();
            foreach (var s in recipe.Steps)
                Steps.Add(s);
        }
        catch
        {
            SetError("Failed to load recipe details.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Recipe == null) return;
        await _favoritesService.ToggleFavoriteAsync(Recipe);
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private void SetRating(string ratingStr)
    {
        if (int.TryParse(ratingStr, out var rating))
        {
            UserRating = rating;
            if (Recipe != null)
                Recipe.Rating = rating;
        }
    }

    [RelayCommand]
    private async Task StartCookingAsync()
    {
        if (Recipe == null) return;
        await Shell.Current.GoToAsync($"CookingMode?recipeId={Recipe.Id}");
    }

    [RelayCommand]
    private async Task ShareRecipeAsync()
    {
        if (Recipe == null) return;

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = Recipe.Title,
            Text = $"Check out this recipe: {Recipe.Title}\n{Recipe.Description}"
        });
    }

    [RelayCommand]
    private void ToggleIngredient(Ingredient ingredient)
    {
        ingredient.IsChecked = !ingredient.IsChecked;

        var idx = Ingredients.IndexOf(ingredient);
        if (idx >= 0)
        {
            Ingredients[idx] = ingredient;
        }
    }
}
