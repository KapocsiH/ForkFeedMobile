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
}

public partial class Ingredient : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Formatted string for UI display (e.g. "200 g", "0.5 ml", or empty).
    /// </summary>
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
