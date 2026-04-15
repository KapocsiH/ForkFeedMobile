using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class ShoppingListViewModel : BaseViewModel
{
    private readonly ShoppingListService _shoppingListService;
    private readonly AuthService _authService;

    public ObservableCollection<ShoppingListItem> Items { get; } = new();

    [ObservableProperty]
    private bool _isEmpty = true;

    public ShoppingListViewModel(ShoppingListService shoppingListService, AuthService authService)
    {
        _shoppingListService = shoppingListService;
        _authService = authService;
        Title = "Shopping List";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        var userId = _authService.CurrentUser?.Id ?? 0;
        if (userId == 0)
        {
            Items.Clear();
            IsEmpty = true;
            return;
        }

        var items = await _shoppingListService.LoadAsync(userId);

        Items.Clear();
        foreach (var item in items)
            Items.Add(item);

        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private async Task RemoveItemAsync(ShoppingListItem item)
    {
        if (item == null) return;

        var userId = _authService.CurrentUser?.Id ?? 0;
        if (userId == 0) return;

        await _shoppingListService.RemoveItemAsync(userId, item);
        Items.Remove(item);
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        var userId = _authService.CurrentUser?.Id ?? 0;
        if (userId == 0) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Clear Shopping List",
            "Are you sure you want to remove all items?",
            "Clear", "Cancel");

        if (!confirm) return;

        await _shoppingListService.ClearAsync(userId);
        Items.Clear();
        IsEmpty = true;
    }

    [RelayCommand]
    private async Task ToggleCheckedAsync(ShoppingListItem item)
    {
        if (item == null) return;

        item.IsChecked = !item.IsChecked;

        var userId = _authService.CurrentUser?.Id ?? 0;
        if (userId == 0) return;

        // Persist the checked state
        var items = new List<ShoppingListItem>(Items);
        await _shoppingListService.SaveAsync(userId, items);

        // Refresh the item in the collection to trigger UI update
        var index = Items.IndexOf(item);
        if (index >= 0)
        {
            Items[index] = item;
        }
    }
}
