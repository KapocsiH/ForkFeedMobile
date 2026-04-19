using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class FavoritesViewModel : BaseViewModel
{
    private readonly FavoritesService _favoritesService;

    public ObservableCollection<Recipe> Favorites { get; } = new();

    [ObservableProperty]
    private bool _isEmpty;

    public FavoritesViewModel(FavoritesService favoritesService)
    {
        _favoritesService = favoritesService;
        Title = "Favorites";
    }

    [RelayCommand]
    private async Task LoadFavoritesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsLoading = Favorites.Count == 0;
            ClearError();

            var favs = await _favoritesService.GetFavoritesAsync();

            Favorites.Clear();
            foreach (var r in favs)
                Favorites.Add(r);

            IsEmpty = Favorites.Count == 0;
        }
        catch (Exception ex)
        {
            SetError($"Unable to load favorites. {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFavoriteAsync(Recipe recipe)
    {
        if (recipe == null) return;

        try
        {
            var success = await _favoritesService.ToggleFavoriteAsync(recipe);

            if (success)
            {
                Favorites.Remove(recipe);
                IsEmpty = Favorites.Count == 0;
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Failed to remove from favorites. Please try again.",
                    "OK");
            }
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert(
                "Error",
                "Something went wrong. Please try again.",
                "OK");
        }
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={recipe.Id}");
    }
}
