namespace ForkFeedMobile.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Easy";
    public int TimeMinutes { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<RecipeStep> Steps { get; set; } = new();
    public bool IsFavorite { get; set; }
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorProfileImageUrl { get; set; } = string.Empty;

    public string DifficultyDisplay => Difficulty?.ToLower() switch
    {
        "easy" => "Könnyű",
        "medium" => "Közepes",
        "hard" => "Nehéz",
        _ => Difficulty ?? string.Empty
    };

    public Color DifficultyBorderColor => Difficulty?.ToLower() switch
    {
        "easy" => Color.FromArgb("#4CAF50"),
        "medium" => Color.FromArgb("#FFC107"),
        "hard" => Color.FromArgb("#F44336"),
        _ => Color.FromArgb("#9E9E9E")
    };

    public Color DifficultyBackgroundColor => Difficulty?.ToLower() switch
    {
        "easy" => Color.FromArgb("#1B5E20"),
        "medium" => Color.FromArgb("#F57F17"),
        "hard" => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#616161")
    };
}

public partial class Ingredient : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string DisplayQuantity =>
        Quantity.HasValue
            ? string.IsNullOrWhiteSpace(Unit)
                ? Quantity.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : $"{Quantity.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)} {Unit}"
            : Unit ?? string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isChecked;
}

public class RecipeStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
