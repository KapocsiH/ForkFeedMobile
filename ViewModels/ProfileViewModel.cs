using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly IApiService _apiService;
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private UserProfile? _user;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _loginError = string.Empty;

    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private int _recipeCount;

    [ObservableProperty]
    private int _collectionCount;

    [ObservableProperty]
    private bool _isProfileLoaded;

    [ObservableProperty]
    private string _bio = string.Empty;

    [ObservableProperty]
    private bool _isBioExpanded;

    [ObservableProperty]
    private bool _hasBio;

    [ObservableProperty]
    private int _bioMaxLines = 3;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRecipesTabSelected))]
    [NotifyPropertyChangedFor(nameof(IsBooksTabSelected))]
    [NotifyPropertyChangedFor(nameof(IsCommentsTabSelected))]
    private string _selectedTab = "Recipes";

    public bool IsRecipesTabSelected => SelectedTab == "Recipes";
    public bool IsBooksTabSelected => SelectedTab == "Books";
    public bool IsCommentsTabSelected => SelectedTab == "Comments";

    public ObservableCollection<Recipe> UserRecipes { get; } = new();

    public ProfileViewModel(AuthService authService, IApiService apiService,
        RecipeService recipeService, FavoritesService favoritesService)
    {
        _authService = authService;
        _apiService = apiService;
        _recipeService = recipeService;
        _favoritesService = favoritesService;
        Title = "Profile";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        LoginError = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            LoginError = "Please enter email and password.";
            return;
        }

        IsBusy = true;

        var (success, error) = await _authService.LoginAsync(Email, Password);

        IsBusy = false;

        if (success)
        {
            RefreshState();
            Email = string.Empty;
            Password = string.Empty;

            // Navigate to Home so the Profile tab gets a fresh OnAppearing
            // the next time the user opens it.
            await Shell.Current.GoToAsync("//Home");

            var toast = Toast.Make("Logged in successfully! ??", ToastDuration.Short, 14);
            await toast.Show();
        }
        else
        {
            LoginError = error ?? "Invalid credentials.";
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        await _authService.LogoutAsync();
        IsBusy = false;
        IsProfileLoaded = false;
        AverageRating = 0;
        RecipeCount = 0;
        CollectionCount = 0;
        Bio = string.Empty;
        HasBio = false;
        IsBioExpanded = false;
        BioMaxLines = 3;
        SelectedTab = "Recipes";
        UserRecipes.Clear();
        RefreshState();
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("ForgotPassword");
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("Register");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        await _authService.TryRestoreSessionAsync();
        IsBusy = false;
        RefreshState();

        if (IsLoggedIn)
        {
            await LoadProfileAsync();
            await LoadUserRecipesAsync();
        }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (!IsLoggedIn)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _apiService.GetMyStatsAsync();

            if (result.IsSuccess && result.Data?.Stats != null)
            {
                var stats = result.Data.Stats;
                AverageRating = Math.Round(stats.AverageRecipeRating, 1);
                RecipeCount = stats.RecipesCount;
                CollectionCount = stats.RecipeBooksCount;
            }
            else
            {
                SetError("Could not load profile stats.");
            }
        }
        catch
        {
            SetError("Failed to load profile data.");
        }
        finally
        {
            IsBusy = false;
            IsProfileLoaded = true;
        }
    }

    [RelayCommand]
    private async Task LoadUserRecipesAsync()
    {
        if (!IsLoggedIn || User == null)
            return;

        try
        {
            var recipes = await _recipeService.GetUserRecipesAsync(User.Id);

            UserRecipes.Clear();
            foreach (var r in recipes)
            {
                r.IsFavorite = await _favoritesService.IsFavoriteAsync(r.Id);
                UserRecipes.Add(r);
            }
        }
        catch
        {
            // Silently fail; the user can retry by switching tabs
        }
    }

    [RelayCommand]
    private async Task GoToRecipeDetailAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={recipe.Id}");
    }

    [RelayCommand]
    private async Task ToggleUserRecipeFavoriteAsync(Recipe recipe)
    {
        await _favoritesService.ToggleFavoriteAsync(recipe);
        recipe.IsFavorite = await _favoritesService.IsFavoriteAsync(recipe.Id);

        var index = UserRecipes.IndexOf(recipe);
        if (index >= 0)
        {
            UserRecipes[index] = recipe;
        }
    }

    [RelayCommand]
    private void SelectTab(string tab)
    {
        SelectedTab = tab;
    }

    [RelayCommand]
    private void ToggleBio()
    {
        IsBioExpanded = !IsBioExpanded;
        BioMaxLines = IsBioExpanded ? int.MaxValue : 3;
    }

    private void RefreshState()
    {
        IsLoggedIn = _authService.IsLoggedIn;
        User = _authService.CurrentUser;
        Bio = User?.Bio ?? string.Empty;
        HasBio = !string.IsNullOrWhiteSpace(Bio);
        IsBioExpanded = false;
        BioMaxLines = 3;
    }
}
