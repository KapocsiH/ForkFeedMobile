using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class RecipeService
{
    private readonly IApiService _api;

    public RecipeService(IApiService api)
    {
        _api = api;
    }

    public async Task<List<Recipe>> GetRecipesAsync(int page = 0, int pageSize = 6,
        string? search = null, string? difficulty = null, string? sortBy = null)
    {
        // API uses 1-based pages; existing callers use 0-based
        var apiPage = page + 1;

        // Map UI sort options to API query params
        string? sort = null;
        string? order = null;
        switch (sortBy)
        {
            case "Date":
                sort = "created_at";
                order = "desc";
                break;
            case "Difficulty":
                sort = "difficulty";
                order = "asc";
                break;
            case "Rating":
                sort = "rating";
                order = "desc";
                break;
        }

        // Map "All" to null so the API returns everything
        var diff = string.IsNullOrWhiteSpace(difficulty) || difficulty == "All"
            ? null
            : difficulty.ToLower();

        var result = await _api.GetRecipesAsync(apiPage, pageSize, search, diff, sort, order);

        if (!result.IsSuccess || result.Data == null)
            return new List<Recipe>();

        return result.Data.Recipes.Select(MapToRecipe).ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        // Fetch summary, ingredients, and steps in parallel
        var summaryTask = _api.GetRecipeSummaryAsync(id);
        var ingredientsTask = _api.GetRecipeIngredientsAsync(id);
        var stepsTask = _api.GetRecipeStepsAsync(id);

        await Task.WhenAll(summaryTask, ingredientsTask, stepsTask);

        var summaryResult = summaryTask.Result;
        if (!summaryResult.IsSuccess || summaryResult.Data?.Summary == null)
            return null;

        var s = summaryResult.Data.Summary;
        var recipe = new Recipe
        {
            Id = s.Id,
            Title = s.Title,
            Difficulty = CapitalizeFirst(s.Difficulty),
            TimeMinutes = s.PreparationTime,
            Rating = s.AverageRating,
        };

        // Try to get the image from the list endpoint (summary doesn't include it)
        var listResult = await _api.GetRecipesAsync(1, 100);
        if (listResult.IsSuccess && listResult.Data != null)
        {
            var match = listResult.Data.Recipes.FirstOrDefault(r => r.Id == id);
            if (match != null)
            {
                recipe.ImageUrl = match.ImageUrl ?? string.Empty;
                recipe.Description = match.Description ?? string.Empty;
                recipe.CreatedAt = match.CreatedAt;
            }
        }

        // Ingredients
        if (ingredientsTask.Result.IsSuccess && ingredientsTask.Result.Data != null)
        {
            recipe.Ingredients = ingredientsTask.Result.Data.Ingredients
                .Select(i => new Ingredient
                {
                    Name = i.Name,
                    Quantity = $"{i.Quantity} {i.Unit}".Trim()
                }).ToList();
        }

        // Steps
        if (stepsTask.Result.IsSuccess && stepsTask.Result.Data != null)
        {
            recipe.Steps = stepsTask.Result.Data.Steps
                .OrderBy(st => st.StepNumber)
                .Select(st => new RecipeStep
                {
                    StepNumber = st.StepNumber,
                    Description = st.Description
                }).ToList();
        }

        return recipe;
    }

    public async Task AddRecipeAsync(Recipe recipe)
    {
        // For now, creating recipes via the API would require auth.
        // This is a placeholder that matches the existing contract.
        await Task.Delay(400);
    }

    // ?? Mapping helpers ??????????????????????????????????????????

    private static Recipe MapToRecipe(ApiRecipe api) => new()
    {
        Id = api.Id,
        Title = api.Title,
        Description = api.Description ?? string.Empty,
        ImageUrl = api.ImageUrl ?? string.Empty,
        Difficulty = CapitalizeFirst(api.Difficulty),
        TimeMinutes = api.PreparationTime,
        Rating = api.AverageRating,
        CreatedAt = api.CreatedAt,
    };

    private static string CapitalizeFirst(string value) =>
        string.IsNullOrEmpty(value) ? value
        : char.ToUpper(value[0]) + value[1..].ToLower();
}
