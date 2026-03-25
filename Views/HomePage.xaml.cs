using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load data when page appears (handles returning from detail too)
        if (_vm.Recipes.Count == 0)
            await _vm.LoadRecipesCommand.ExecuteAsync(null);
    }
}
