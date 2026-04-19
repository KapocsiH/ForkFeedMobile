using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _registerError = string.Empty;

    public RegisterViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Create Account";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        RegisterError = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            RegisterError = "Please fill in all fields.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            RegisterError = "Passwords do not match.";
            return;
        }

        IsBusy = true;

        var (success, error) = await _authService.RegisterAsync(Username, Email, Password);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            RegisterError = error ?? "Registration failed.";
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
