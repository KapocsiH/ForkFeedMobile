using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private UserProfile? _user;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _loginError = string.Empty;

    public ProfileViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Profile";
        RefreshState();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        LoginError = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            LoginError = "Please enter email and password.";
            return;
        }

        IsBusy = true;

        var success = await _authService.LoginAsync(Email, Password);

        IsBusy = false;

        if (success)
        {
            RefreshState();
            Email = string.Empty;
            Password = string.Empty;
        }
        else
        {
            LoginError = "Invalid credentials.";
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        RefreshState();
    }

    [RelayCommand]
    private void Refresh()
    {
        RefreshState();
    }

    private void RefreshState()
    {
        IsLoggedIn = _authService.IsLoggedIn;
        User = _authService.CurrentUser;
    }
}
