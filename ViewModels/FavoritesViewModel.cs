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
        IsBusy = true;

        var favs = await _favoritesService.GetFavoritesAsync();

        Favorites.Clear();
        foreach (var r in favs)
            Favorites.Add(r);

        IsEmpty = Favorites.Count == 0;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task RemoveFavoriteAsync(Recipe recipe)
    {
        await _favoritesService.ToggleFavoriteAsync(recipe);
        Favorites.Remove(recipe);
        IsEmpty = Favorites.Count == 0;
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await Shell.Current.GoToAsync($"RecipeDetail?recipeId={recipe.Id}");
    }
}
