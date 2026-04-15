using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class ShoppingListPage : ContentPage
{
    public ShoppingListPage(ShoppingListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
