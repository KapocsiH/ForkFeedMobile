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

        var (success, error) = await _authService.LoginAsync(Email, Password);

        IsBusy = false;

        if (success)
        {
            RefreshState();
            Email = string.Empty;
            Password = string.Empty;
        }
        else
        {
            LoginError = error ?? "Invalid credentials.";
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        await _authService.LogoutAsync();
        IsBusy = false;
        RefreshState();
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("ForgotPassword");
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("Register");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        await _authService.TryRestoreSessionAsync();
        IsBusy = false;
        RefreshState();
    }

    private void RefreshState()
    {
        IsLoggedIn = _authService.IsLoggedIn;
        User = _authService.CurrentUser;
    }
}
