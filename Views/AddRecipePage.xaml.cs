using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class AddRecipePage : ContentPage
{
    public AddRecipePage(AddRecipeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
