using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    // ?? Forgot password step ?????????????????????????????????????

    [ObservableProperty]
    private string _email = string.Empty;

    // ?? Reset password step ??????????????????????????????????????

    [ObservableProperty]
    private string _resetToken = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    // ?? UI state ?????????????????????????????????????????????????

    [ObservableProperty]
    private string _formError = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _showResetForm;

    public ForgotPasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Forgot Password";
    }

    [RelayCommand]
    private async Task SendResetLinkAsync()
    {
        FormError = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
        {
            FormError = "Please enter your email address.";
            return;
        }

        IsBusy = true;

        var (success, error) = await _authService.ForgotPasswordAsync(Email);

        IsBusy = false;

        if (success)
        {
            SuccessMessage = "If this email is registered, you will receive a reset link shortly.";
            ShowResetForm = true;
        }
        else
        {
            FormError = error ?? "Failed to send reset link.";
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        FormError = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ResetToken) || string.IsNullOrWhiteSpace(NewPassword))
        {
            FormError = "Please fill in all fields.";
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            FormError = "Passwords do not match.";
            return;
        }

        IsBusy = true;

        var (success, error) = await _authService.ResetPasswordAsync(ResetToken, NewPassword);

        IsBusy = false;

        if (success)
        {
            await Shell.Current.DisplayAlert("Success", "Your password has been reset. Please log in.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            FormError = error ?? "Password reset failed.";
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
