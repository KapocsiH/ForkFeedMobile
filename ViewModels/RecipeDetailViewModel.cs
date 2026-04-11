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
    private Recipe _recipe = new();

    [ObservableProperty]
    private bool _isRecipeLoaded;

    [ObservableProperty]
    private int _userRating;

    [ObservableProperty]
    private bool _hasNoComments;

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();
    public ObservableCollection<Comment> Comments { get; } = new();

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
            IsRecipeLoaded = true;

            Ingredients.Clear();
            foreach (var i in recipe.Ingredients)
                Ingredients.Add(new Ingredient { Name = i.Name, Quantity = i.Quantity });

            Steps.Clear();
            foreach (var s in recipe.Steps)
                Steps.Add(s);

            Comments.Clear();
            var comments = await _recipeService.GetCommentsByRecipeIdAsync(RecipeId);
            foreach (var c in comments)
                Comments.Add(c);

            HasNoComments = Comments.Count == 0;
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
        if (!IsRecipeLoaded) return;
        await _favoritesService.ToggleFavoriteAsync(Recipe);
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private void SetRating(string ratingStr)
    {
        if (int.TryParse(ratingStr, out var rating))
        {
            UserRating = rating;
            if (IsRecipeLoaded)
                Recipe.Rating = rating;
        }
    }

    [RelayCommand]
    private async Task StartCookingAsync()
    {
        if (!IsRecipeLoaded) return;
        await Shell.Current.GoToAsync($"CookingMode?recipeId={Recipe.Id}");
    }

    [RelayCommand]
    private async Task ShareRecipeAsync()
    {
        if (!IsRecipeLoaded) return;

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

    [RelayCommand]
    private async Task AuthorTappedAsync()
    {
        if (!IsRecipeLoaded || Recipe.AuthorId <= 0) return;
        await Shell.Current.GoToAsync($"UserProfile?userId={Recipe.AuthorId}");
    }

    [RelayCommand]
    private async Task SaveToCookbookAsync()
    {
        await Shell.Current.DisplayAlert("Save to Cookbook", "This feature is coming soon!", "OK");
    }

    [RelayCommand]
    private async Task ReportRecipeAsync()
    {
        await Shell.Current.DisplayAlert("Report", "This feature is coming soon!", "OK");
    }

    [RelayCommand]
    private async Task ReportCommentAsync()
    {
        await Shell.Current.DisplayAlert("Report", "This feature is coming soon!", "OK");
    }
}
