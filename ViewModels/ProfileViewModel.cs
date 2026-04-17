using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class ProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly IApiService _apiService;
    private readonly RecipeService _recipeService;
    private readonly FavoritesService _favoritesService;

    [ObservableProperty]
    private int _userId;

    [ObservableProperty]
    private bool _isOwnProfile = true;

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
    private bool _isBioTruncated;

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
    public ObservableCollection<RecipeBook> UserRecipeBooks { get; } = new();
    public ObservableCollection<UserComment> UserComments { get; } = new();

    public ProfileViewModel(AuthService authService, IApiService apiService,
        RecipeService recipeService, FavoritesService favoritesService)
    {
        _authService = authService;
        _apiService = apiService;
        _recipeService = recipeService;
        _favoritesService = favoritesService;
        Title = "Profile";
    }

    partial void OnUserIdChanged(int value)
    {
        if (value > 0)
        {
            IsOwnProfile = false;
            _ = LoadExternalProfileAsync(value);
        }
    }

    private async Task LoadExternalProfileAsync(int userId)
    {
        try
        {
            IsBusy = true;
            ClearError();
            IsLoggedIn = true;

            var userResult = await _apiService.GetUserAsync(userId);
            if (userResult.IsSuccess && userResult.Data?.User != null)
            {
                var apiUser = userResult.Data.User;
                User = new UserProfile
                {
                    Id = apiUser.Id,
                    DisplayName = apiUser.Username,
                    Email = string.Empty,
                    AvatarUrl = apiUser.ProfileImageUrl ?? string.Empty,
                    Bio = apiUser.Bio ?? string.Empty,
                    MemberSince = apiUser.CreatedAt ?? DateTime.Now
                };
                Title = apiUser.Username;
                Bio = User.Bio;
                HasBio = !string.IsNullOrWhiteSpace(Bio);
            }
            else
            {
                SetError("Could not load user profile.");
                return;
            }

            var statsResult = await _apiService.GetUserStatsAsync(userId);
            if (statsResult.IsSuccess && statsResult.Data?.Stats != null)
            {
                var stats = statsResult.Data.Stats;
                AverageRating = Math.Round(stats.AverageRecipeRating, 1);
                RecipeCount = stats.RecipesCount;
                CollectionCount = stats.RecipeBooksCount;
            }

            var recipes = await _recipeService.GetUserRecipesAsync(userId);
            UserRecipes.Clear();
            foreach (var r in recipes)
            {
                r.IsFavorite = await _favoritesService.IsFavoriteAsync(r.Id);
                UserRecipes.Add(r);
            }

            var comments = await _recipeService.GetUserCommentsWithRecipeInfoAsync(userId);
            UserComments.Clear();
            foreach (var c in comments)
                UserComments.Add(c);

            var booksResult = await _apiService.GetUserRecipeBooksAsync(userId);
            UserRecipeBooks.Clear();
            if (booksResult.IsSuccess && booksResult.Data != null)
            {
                foreach (var b in booksResult.Data.RecipeBooks)
                    UserRecipeBooks.Add(new RecipeBook
                    {
                        Id = b.Id,
                        Title = b.Name,
                        Description = b.Description ?? string.Empty,
                        RecipeCount = b.RecipeCount,
                        CreatedAt = b.CreatedAt,
                        IsPublic = b.IsPublic,
                        IsOwn = false
                    });
            }
        }
        catch
        {
            SetError("Failed to load user profile.");
        }
        finally
        {
            IsBusy = false;
            IsProfileLoaded = true;
        }
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
        IsBioTruncated = false;
        BioMaxLines = 3;
        SelectedTab = "Recipes";
        UserRecipes.Clear();
        UserRecipeBooks.Clear();
        UserComments.Clear();
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
            await LoadUserRecipeBooksAsync();
            await LoadUserCommentsAsync();
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
    private async Task LoadUserRecipeBooksAsync()
    {
        if (!IsLoggedIn || User == null)
            return;

        try
        {
            var result = await _apiService.GetUserRecipeBooksAsync(User.Id);
            UserRecipeBooks.Clear();
            if (result.IsSuccess && result.Data != null)
            {
                foreach (var b in result.Data.RecipeBooks)
                    UserRecipeBooks.Add(new RecipeBook
                    {
                        Id = b.Id,
                        Title = b.Name,
                        Description = b.Description ?? string.Empty,
                        RecipeCount = b.RecipeCount,
                        CreatedAt = b.CreatedAt,
                        IsPublic = b.IsPublic,
                        IsOwn = true
                    });
            }
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task LoadUserCommentsAsync()
    {
        if (!IsLoggedIn || User == null)
            return;

        try
        {
            var comments = await _recipeService.GetUserCommentsWithRecipeInfoAsync(User.Id);

            UserComments.Clear();
            foreach (var c in comments)
                UserComments.Add(c);
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
    private async Task GoToCommentRecipeDetailAsync(UserComment comment)
    {
        if (comment == null) return;
        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={comment.RecipeId}");
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
    private async Task GoToRecipeBookDetailAsync(RecipeBook book)
    {
        if (book == null) return;
        await Shell.Current.GoToAsync($"RecipeBookDetails?bookId={book.Id}&bookTitle={Uri.EscapeDataString(book.Title)}&bookDescription={Uri.EscapeDataString(book.Description)}&isPublic={book.IsPublic}&isOwn={book.IsOwn}");
    }

    [RelayCommand]
    private async Task GoToEditProfileAsync()
    {
        if (User == null) return;

        var parameters = new Dictionary<string, object>
        {
            { "username", User.DisplayName },
            { "bio", User.Bio ?? string.Empty },
            { "avatarUrl", User.AvatarUrl ?? string.Empty }
        };

        await Shell.Current.GoToAsync("EditProfile", parameters);
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
        IsBioTruncated = false;
        BioMaxLines = 3;
    }
}
