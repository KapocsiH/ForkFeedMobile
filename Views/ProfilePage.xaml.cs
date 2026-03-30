using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _vm;
    private bool _hasRestoredSession;

    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_hasRestoredSession)
        {
            _hasRestoredSession = true;
        }

        if (_vm.RefreshCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand asyncCmd)
            await asyncCmd.ExecuteAsync(null);
        else
            _vm.RefreshCommand.Execute(null);

        if (_vm.IsLoggedIn)
        {
            await AnimateHeaderAsync();
        }
    }

    private async Task AnimateHeaderAsync()
    {
        ProfileHeader.Opacity = 0;
        await ProfileHeader.FadeTo(1, 500, Easing.CubicIn);
    }
}
