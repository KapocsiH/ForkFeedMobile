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
    private readonly AuthService _authService;

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

    [ObservableProperty]
    private string _newCommentText = string.Empty;

    [ObservableProperty]
    private bool _isPostingComment;

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();
    public ObservableCollection<Comment> Comments { get; } = new();

    public RecipeDetailViewModel(RecipeService recipeService, FavoritesService favoritesService, AuthService authService)
    {
        _recipeService = recipeService;
        _favoritesService = favoritesService;
        _authService = authService;
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
            var currentUserId = _authService.CurrentUser?.Id;
            var comments = await _recipeService.GetCommentsByRecipeIdAsync(RecipeId, currentUserId);
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

    [RelayCommand]
    private async Task DeleteCommentAsync(Comment comment)
    {
        if (comment == null || comment.Id <= 0)
            return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete Comment",
            "Are you sure you want to delete this comment?",
            "Delete", "Cancel");

        if (!confirm)
            return;

        try
        {
            var success = await _recipeService.DeleteCommentAsync(comment.Id);
            if (success)
            {
                Comments.Remove(comment);
                HasNoComments = Comments.Count == 0;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to delete comment. Please try again.", "OK");
            }
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
        }
    }

    [RelayCommand]
    private async Task PostCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCommentText))
            return;

        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Login Required", "Please log in to post a comment.", "OK");
            return;
        }

        try
        {
            IsPostingComment = true;
            ClearError();

            var comment = await _recipeService.CreateCommentAsync(RecipeId, NewCommentText.Trim());

            if (comment == null)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to post comment. Please try again.", "OK");
                return;
            }

            Comments.Add(comment);
            HasNoComments = false;
            NewCommentText = string.Empty;
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
        }
        finally
        {
            IsPostingComment = false;
        }
    }
}
