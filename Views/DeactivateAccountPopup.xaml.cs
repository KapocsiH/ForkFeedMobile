using CommunityToolkit.Maui.Views;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.Views;

public partial class DeactivateAccountPopup : Popup
{
    private readonly IApiService _apiService;
    private readonly AuthService _authService;

    public DeactivateAccountPopup(IApiService apiService, AuthService authService)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter your password.");
            return;
        }

        SetLoading(true);
        ErrorLabel.IsVisible = false;

        // Validate password by attempting login with current user's email
        var email = _authService.CurrentUser?.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError("Could not verify your identity.");
            SetLoading(false);
            return;
        }

        var loginResult = await _apiService.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = password
        });

        if (!loginResult.IsSuccess)
        {
            ShowError("Incorrect password.");
            SetLoading(false);
            return;
        }

        // Re-set the token from the login response (in case it changed)
        if (loginResult.Data?.Token != null && _apiService is ApiService apiService)
            apiService.SetAuthToken(loginResult.Data.Token);

        // Deactivate the account
        var deactivateResult = await _apiService.DeactivateMyAccountAsync(password);

        if (!deactivateResult.IsSuccess)
        {
            ShowError(deactivateResult.ErrorMessage ?? "Failed to deactivate account.");
            SetLoading(false);
            return;
        }

        SetLoading(false);
        await CloseAsync(true);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CloseAsync(false);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        ConfirmButton.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;
    }
}
