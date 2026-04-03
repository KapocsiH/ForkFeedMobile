using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class AuthService
{
    private readonly IApiService _apiService;
    private bool _isLoggedIn;
    private UserProfile? _currentUser;
    private string? _authToken;

    public bool IsLoggedIn => _isLoggedIn;
    public UserProfile? CurrentUser => _currentUser;

    public AuthService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Please enter email and password.");

        var result = await _apiService.LoginAsync(new LoginRequest
        {
            Email = email.Trim(),
            Password = password
        });

        if (!result.IsSuccess || result.Data == null)
            return (false, result.ErrorMessage ?? "Login failed.");

        var auth = result.Data;
        if (string.IsNullOrEmpty(auth.Token) || auth.User == null)
            return (false, "Invalid server response.");

        _authToken = auth.Token;
        SetTokenOnApiService(auth.Token);
        await SaveTokenAsync(auth.Token);

        _currentUser = MapToUserProfile(auth.User);
        _isLoggedIn = true;

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Please fill in all fields.");

        var result = await _apiService.RegisterAsync(new RegisterRequest
        {
            Username = username.Trim(),
            Email = email.Trim(),
            Password = password
        });

        if (!result.IsSuccess || result.Data == null)
            return (false, result.ErrorMessage ?? "Registration failed.");

        var auth = result.Data;
        if (string.IsNullOrEmpty(auth.Token) || auth.User == null)
            return (false, "Invalid server response.");

        _authToken = auth.Token;
        SetTokenOnApiService(auth.Token);
        await SaveTokenAsync(auth.Token);

        _currentUser = MapToUserProfile(auth.User);
        _isLoggedIn = true;

        return (true, null);
    }

    public async Task LogoutAsync()
    {
        if (!string.IsNullOrEmpty(_authToken))
            await _apiService.LogoutAsync();

        _isLoggedIn = false;
        _currentUser = null;
        _authToken = null;
        SetTokenOnApiService(null);
        await RemoveTokenAsync();
    }

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Please enter your email address.");

        var result = await _apiService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = email.Trim()
        });

        if (!result.IsSuccess)
            return (false, result.ErrorMessage ?? "Request failed.");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Please fill in all fields.");

        var result = await _apiService.ResetPasswordAsync(new ResetPasswordRequest
        {
            Token = token.Trim(),
            NewPassword = newPassword
        });

        if (!result.IsSuccess)
            return (false, result.ErrorMessage ?? "Password reset failed.");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Please fill in all fields.");

        var result = await _apiService.ChangePasswordAsync(new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        });

        if (!result.IsSuccess)
            return (false, result.ErrorMessage ?? "Password change failed.");

        return (true, null);
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await GetSavedTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        _authToken = token;
        SetTokenOnApiService(token);

        var result = await _apiService.GetMeAsync();
        if (!result.IsSuccess || result.Data?.User == null)
        {
            _authToken = null;
            SetTokenOnApiService(null);
            await RemoveTokenAsync();
            return false;
        }

        _currentUser = MapToUserProfile(result.Data.User);
        _isLoggedIn = true;
        return true;
    }

    private void SetTokenOnApiService(string? token)
    {
        if (_apiService is ApiService apiService)
            apiService.SetAuthToken(token);
    }

    private static UserProfile MapToUserProfile(ApiUser user) => new()
    {
        Id = user.Id,
        DisplayName = user.Username,
        Email = user.Email ?? string.Empty,
        AvatarUrl = user.ProfileImageUrl ?? string.Empty,
        Bio = user.Bio ?? string.Empty,
        MemberSince = user.CreatedAt ?? DateTime.Now
    };

    private static async Task SaveTokenAsync(string token)
    {
        try { await SecureStorage.Default.SetAsync("auth_token", token); }
        catch { /* SecureStorage may not be available on all platforms */ }
    }

    private static async Task<string?> GetSavedTokenAsync()
    {
        try { return await SecureStorage.Default.GetAsync("auth_token"); }
        catch { return null; }
    }

    private static Task RemoveTokenAsync()
    {
        try { SecureStorage.Default.Remove("auth_token"); }
        catch { /* ignore */ }
        return Task.CompletedTask;
    }
}
