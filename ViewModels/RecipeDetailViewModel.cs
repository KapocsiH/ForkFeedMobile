using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;
using ForkFeedMobile.Views;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;
    private readonly AuthService _authService;
    private readonly ShoppingListService _shoppingListService;
    private readonly IApiService _apiService;

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

    [ObservableProperty]
    private bool _isSubmittingRating;

    [ObservableProperty]
    private bool _hasSelectedIngredients;

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<Ingredient> SelectedIngredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();
    public ObservableCollection<Comment> Comments { get; } = new();

    public RecipeDetailViewModel(RecipeService recipeService, FavoritesService favoritesService, AuthService authService, ShoppingListService shoppingListService, IApiService apiService)
    {
        _recipeService = recipeService;
        _favoritesService = favoritesService;
        _authService = authService;
        _shoppingListService = shoppingListService;
        _apiService = apiService;
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
            IsLoading = true;
            ClearError();

            var recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (recipe == null)
            {
                SetError("Recipe not found.");
                return;
            }

            recipe.IsFavorite = _favoritesService.IsFavoriteAsync(recipe.Id).Result;
            Recipe = recipe;
            Title = recipe.Title;
            IsRecipeLoaded = true;
            Ingredients.Clear();
            SelectedIngredients.Clear();
            HasSelectedIngredients = false;
            foreach (var i in recipe.Ingredients)
            {
                var ingredient = new Ingredient { Name = i.Name, Quantity = i.Quantity, Unit = i.Unit };
                ingredient.PropertyChanged += OnIngredientPropertyChanged;
                Ingredients.Add(ingredient);
            }

            Steps.Clear();
            foreach (var s in recipe.Steps)
                Steps.Add(s);
            var currentUserId = _authService.CurrentUser?.Id;
            var ratingTask = _authService.IsLoggedIn
                ? _recipeService.GetMyRatingAsync(RecipeId)
                : Task.FromResult(0);
            var commentsTask = _recipeService.GetCommentsByRecipeIdAsync(RecipeId, currentUserId);

            await Task.WhenAll(ratingTask, commentsTask);

            UserRating = ratingTask.Result;

            Comments.Clear();
            foreach (var c in commentsTask.Result)
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
            IsLoading = false;
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
    private async Task SetRatingAsync(string ratingStr)
    {
        if (!int.TryParse(ratingStr, out var rating))
            return;

        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Login Required", "Please log in to rate recipes.", "OK");
            return;
        }

        if (!IsRecipeLoaded || IsSubmittingRating)
            return;

        var previousRating = UserRating;
        UserRating = rating;

        try
        {
            IsSubmittingRating = true;

            var (success, updatedAverage) = await _recipeService.RateRecipeAsync(RecipeId, rating);

            if (!success)
            {
                UserRating = previousRating;
                await Shell.Current.DisplayAlert("Error", "Failed to submit rating. Please try again.", "OK");
                return;
            }

            if (updatedAverage.HasValue)
            {
                Recipe.Rating = updatedAverage.Value;
                OnPropertyChanged(nameof(Recipe));
            }
        }
        catch
        {
            UserRating = previousRating;
            await Shell.Current.DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
        }
        finally
        {
            IsSubmittingRating = false;
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

        var recipeUrl = $"https://forkfeed.vercel.app/pages/recipe/{Recipe.Id}";

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = Recipe.Title,
            Text = $"Check out this recipe: {recipeUrl}"
        });
    }

    [RelayCommand]
    private void ToggleIngredient(Ingredient ingredient)
    {
        ingredient.IsChecked = !ingredient.IsChecked;
    }

    private void OnIngredientPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Ingredient.IsChecked) && sender is Ingredient ingredient)
        {
            if (ingredient.IsChecked && !SelectedIngredients.Contains(ingredient))
                SelectedIngredients.Add(ingredient);
            else if (!ingredient.IsChecked)
                SelectedIngredients.Remove(ingredient);

            HasSelectedIngredients = SelectedIngredients.Count > 0;
        }
    }

    [RelayCommand]
    private async Task GoToShoppingListAsync()
    {
        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Login Required", "Please log in to use the shopping list.", "OK");
            return;
        }

        var userId = _authService.CurrentUser?.Id ?? 0;
        if (userId == 0) return;

        var selected = SelectedIngredients.ToList();
        if (selected.Count > 0)
        {
            await _shoppingListService.AddIngredientsAsync(userId, selected);
        }

        await Shell.Current.GoToAsync("//ShoppingList");
    }

    [RelayCommand]
    private async Task AuthorTappedAsync()
    {
        if (!IsRecipeLoaded || Recipe.AuthorId <= 0) return;
        await Shell.Current.GoToAsync($"UserProfile?userId={Recipe.AuthorId}");
    }

    [RelayCommand]
    private async Task CommentAuthorTappedAsync(Comment comment)
    {
        if (comment == null || comment.UserId <= 0) return;
        await Shell.Current.GoToAsync($"UserProfile?userId={comment.UserId}");
    }

    [RelayCommand]
    private async Task SaveToCookbookAsync()
    {
        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Bejelentkezés szükséges", "Kérjük, jelentkezz be.", "OK");
            return;
        }

        var userId = _authService.CurrentUser?.Id ?? 0;
        var popup = new SaveToRecipeBookPopup(_apiService, RecipeId, userId);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is true)
        {
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make("Recept mentve!", ToastDuration.Short, 14);
            await toast.Show();
        }
    }

    [RelayCommand]
    private async Task ReportRecipeAsync()
    {
        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Login Required", "Please log in to report.", "OK");
            return;
        }

        var popup = new ReportPopup(_apiService, "recipe", RecipeId);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is true)
        {
            var toast = Toast.Make("Report submitted", ToastDuration.Short, 14);
            await toast.Show();
        }
    }

    [RelayCommand]
    private async Task ReportCommentAsync(Comment comment)
    {
        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Login Required", "Please log in to report.", "OK");
            return;
        }

        if (comment == null || comment.Id <= 0) return;

        var popup = new ReportPopup(_apiService, "comment", comment.Id);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is true)
        {
            var toast = Toast.Make("Report submitted", ToastDuration.Short, 14);
            await toast.Show();
        }
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
