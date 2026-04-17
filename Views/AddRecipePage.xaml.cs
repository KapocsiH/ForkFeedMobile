using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class AddRecipePage : ContentPage
{
    private readonly AddRecipeViewModel _vm;

    public AddRecipePage(AddRecipeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataCommand.ExecuteAsync(null);
    }
}
