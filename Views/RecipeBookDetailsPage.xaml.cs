using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class RecipeBookDetailsPage : ContentPage
{
    public RecipeBookDetailsPage(RecipeBookDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
