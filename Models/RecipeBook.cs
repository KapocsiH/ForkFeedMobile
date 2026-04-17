namespace ForkFeedMobile.Models;

public class RecipeBook
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public string RecipeCountDisplay => RecipeCount == 1 ? "1 recipe" : $"{RecipeCount} recipes";
    public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd");
}
