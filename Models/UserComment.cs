namespace ForkFeedMobile.Models;

public class UserComment
{
    public int RecipeId { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public string RecipeAuthorUsername { get; set; } = string.Empty;
    public string RecipeAuthorProfileImageUrl { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
}
