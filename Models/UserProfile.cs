namespace ForkFeedMobile.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
    public DateTime MemberSince { get; set; } = DateTime.Now;
}
