using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(EditProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
