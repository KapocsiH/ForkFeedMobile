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
}

public class Ingredient
{
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
}

public class RecipeStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
