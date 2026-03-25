using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class AuthService
{
    private bool _isLoggedIn;
    private UserProfile? _currentUser;

    public bool IsLoggedIn => _isLoggedIn;
    public UserProfile? CurrentUser => _currentUser;

    public async Task<bool> LoginAsync(string email, string password)
    {
        await Task.Delay(800);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return false;

        _isLoggedIn = true;
        _currentUser = new UserProfile
        {
            Id = 1,
            DisplayName = "Chef Gordon",
            Email = email,
            AvatarUrl = "https://i.pravatar.cc/150?img=12",
            RecipeCount = 24,
            MemberSince = new DateTime(2023, 3, 15)
        };

        return true;
    }

    public Task LogoutAsync()
    {
        _isLoggedIn = false;
        _currentUser = null;
        return Task.CompletedTask;
    }
}
