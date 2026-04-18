using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Helpers;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;
    private readonly DebounceHelper _debounce = new();

    private int _currentPage;
    private bool _hasMoreItems = true;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedDifficulty = "All";

    [ObservableProperty]
    private string _selectedSort = "Date";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isEmpty;

    public List<string> DifficultyOptions { get; } = new() { "All", "Easy", "Medium", "Hard" };
    public List<string> SortOptions { get; } = new() { "Date", "Difficulty", "Rating" };

    public HomeViewModel(RecipeService recipeService, FavoritesService favoritesService)
    {
        _recipeService = recipeService;
        _favoritesService = favoritesService;
        Title = "ForkFeed";
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsLoading = Recipes.Count == 0;
            ClearError();
            IsEmpty = false;

            _currentPage = 0;
            _hasMoreItems = true;

            var recipes = await _recipeService.GetRecipesAsync(
                _currentPage, search: SearchText,
                difficulty: SelectedDifficulty, sortBy: SelectedSort);

            Recipes.Clear();
            foreach (var r in recipes)
            {
                r.IsFavorite = await _favoritesService.IsFavoriteAsync(r.Id);
                Recipes.Add(r);
            }

            if (recipes.Count == 0)
            {
                _hasMoreItems = false;
                IsEmpty = true;
            }
        }
        catch (Exception ex)
        {
            SetError($"Unable to load recipes. {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || !_hasMoreItems) return;

        try
        {
            IsBusy = true;
            _currentPage++;

            var recipes = await _recipeService.GetRecipesAsync(
                _currentPage, search: SearchText,
                difficulty: SelectedDifficulty, sortBy: SelectedSort);

            if (recipes.Count == 0)
            {
                _hasMoreItems = false;
                return;
            }

            foreach (var r in recipes)
            {
                r.IsFavorite = await _favoritesService.IsFavoriteAsync(r.Id);
                Recipes.Add(r);
            }
        }
        catch
        {
            // Silently fail on load-more; user can scroll again to retry
            _currentPage--;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = _debounce.DebounceAsync(async () =>
        {
            await LoadRecipesAsync();
        });
    }

    partial void OnSelectedDifficultyChanged(string value) => _ = LoadRecipesAsync();
    partial void OnSelectedSortChanged(string value) => _ = LoadRecipesAsync();

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Recipe recipe)
    {
        var success = await _favoritesService.ToggleFavoriteAsync(recipe);

        if (success)
        {
            var index = Recipes.IndexOf(recipe);
            if (index >= 0)
            {
                Recipes[index] = recipe;
            }
        }
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Recipe recipe)
    {
        if (recipe == null) return;

        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={recipe.Id}");
    }

    [RelayCommand]
    private async Task RetryAsync()
    {
        await LoadRecipesAsync();
    }
}
