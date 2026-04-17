namespace ForkFeedMobile.Models;

public class RecipeBook
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public bool IsOwn { get; set; }

    public string RecipeCountDisplay => RecipeCount == 1 ? "1 recipe" : $"{RecipeCount} recipes";
    public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd");
    public string VisibilityText => IsPublic ? "Public" : "Private";
    public Color VisibilityBorderColor => IsPublic ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828");
    public Color VisibilityBackgroundColor => IsPublic ? Color.FromArgb("#1B5E20") : Color.FromArgb("#B71C1C");
}
