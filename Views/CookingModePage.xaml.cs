using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class CookingModePage : ContentPage
{
    public CookingModePage(CookingModeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        DeviceDisplay.Current.KeepScreenOn = false;
    }
}
