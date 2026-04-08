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

    public async Task<List<Recipe>> GetUserRecipesAsync(int userId, int page = 0, int pageSize = 20)
    {
        var apiPage = page + 1;
        var result = await _api.GetUserRecipesAsync(userId, apiPage, pageSize);

        if (!result.IsSuccess || result.Data == null)
            return new List<Recipe>();

        return result.Data.Recipes.Select(MapToRecipe).ToList();
    }

    public async Task<List<UserComment>> GetUserCommentsWithRecipeInfoAsync(int userId)
    {
        var comments = new List<UserComment>();

        // Get all recipes to cross-reference comments with recipe info
        var recipesResult = await _api.GetRecipesAsync(1, 100);
        if (!recipesResult.IsSuccess || recipesResult.Data == null)
            return comments;

        var recipes = recipesResult.Data.Recipes;

        foreach (var recipe in recipes)
        {
            var commentsResult = await _api.GetRecipeCommentsAsync(recipe.Id, 1, 100);
            if (!commentsResult.IsSuccess || commentsResult.Data == null)
                continue;

            var userComments = commentsResult.Data.Comments
                .Where(c => c.User?.Id == userId);

            foreach (var c in userComments)
            {
                comments.Add(new UserComment
                {
                    RecipeId = recipe.Id,
                    RecipeTitle = recipe.Title,
                    RecipeAuthorUsername = recipe.Author?.Username ?? "Unknown",
                    RecipeAuthorProfileImageUrl = ResolveImageUrl(recipe.Author?.ProfileImageUrl),
                    CommentText = c.Content
                });
            }
        }

        return comments;
    }

    public async Task<(bool Success, int? RecipeId, string? Error)> CreateRecipeAsync(
        string title, string description, string difficulty, int preparationTime,
        List<Ingredient> ingredients, List<RecipeStep> steps,
        string? imagePath)
    {
        // 1. Create the recipe (with optional image via multipart/form-data)
        Stream? imageStream = null;
        string? imageFileName = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                imageStream = File.OpenRead(imagePath);
                imageFileName = Path.GetFileName(imagePath);
            }

            var request = new CreateRecipeRequest
            {
                Title = title,
                Description = description,
                Difficulty = difficulty.ToLower(),
                PreparationTime = preparationTime
            };

            var createResult = await _api.CreateRecipeAsync(request, imageStream, imageFileName);

            if (!createResult.IsSuccess || createResult.Data?.Recipe == null)
                return (false, null, createResult.ErrorMessage ?? "Failed to create recipe.");

            var recipeId = createResult.Data.Recipe.Id;

            // 2. Add ingredients one by one
            foreach (var ing in ingredients)
            {
                double qty = 0;
                string unit = ing.Quantity;

                var parts = ing.Quantity.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1 && double.TryParse(parts[0], out var parsedQty))
                {
                    qty = parsedQty;
                    unit = parts.Length >= 2 ? parts[1] : "pcs";
                }

                await _api.AddRecipeIngredientAsync(recipeId, new ApiIngredient
                {
                    Name = ing.Name,
                    Quantity = qty,
                    Unit = unit
                });
            }

            // 3. Add steps one by one
            foreach (var step in steps)
            {
                await _api.AddRecipeStepAsync(recipeId, new ApiStep
                {
                    StepNumber = step.StepNumber,
                    Description = step.Description
                });
            }

            return (true, recipeId, null);
        }
        finally
        {
            imageStream?.Dispose();
        }
    }

    // ?? Mapping helpers ??????????????????????????????????????????

    private const string BaseUrl = "https://forkfeed.vercel.app";

    private static Recipe MapToRecipe(ApiRecipe api) => new()
    {
        Id = api.Id,
        Title = api.Title,
        Description = api.Description ?? string.Empty,
        ImageUrl = ResolveImageUrl(api.ImageUrl),
        Difficulty = CapitalizeFirst(api.Difficulty),
        TimeMinutes = api.PreparationTime,
        Rating = api.AverageRating,
        CreatedAt = api.CreatedAt,
    };

    private static string ResolveImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // Already absolute
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        // Relative path — resolve against the API base
        return $"{BaseUrl}{(url.StartsWith('/') ? url : "/" + url)}";
    }

    private static string CapitalizeFirst(string value) =>
        string.IsNullOrEmpty(value) ? value
        : char.ToUpper(value[0]) + value[1..].ToLower();
}
