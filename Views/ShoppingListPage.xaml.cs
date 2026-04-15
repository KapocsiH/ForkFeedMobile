using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _vm;

    public ShoppingListPage(ShoppingListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadItemsCommand.ExecuteAsync(null);
    }
}
