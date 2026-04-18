using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class RecipeService
{
    private readonly IApiService _api;
    private readonly AuthService _authService;

    public RecipeService(IApiService api, AuthService authService)
    {
        _api = api;
        _authService = authService;
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

        // The API does not support server-side text search, so when a search
        // term is provided we fetch a larger batch and filter client-side.
        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var fetchLimit = hasSearch ? 200 : pageSize;
        var fetchPage = hasSearch ? 1 : apiPage;

        var result = await _api.GetRecipesAsync(fetchPage, fetchLimit, null, diff, sort, order);

        if (!result.IsSuccess || result.Data == null)
            return new List<Recipe>();

        IEnumerable<Recipe> recipes = result.Data.Recipes.Select(MapToRecipe);

        if (hasSearch)
        {
            recipes = recipes.Where(r =>
                r.Title.Contains(search!, StringComparison.OrdinalIgnoreCase));

            // Apply manual pagination over the filtered set
            recipes = recipes.Skip(page * pageSize).Take(pageSize);
        }

        return recipes.ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        // Fetch full recipe, ingredients, and steps in parallel
        var recipeTask = _api.GetRecipeAsync(id);
        var ingredientsTask = _api.GetRecipeIngredientsAsync(id);
        var stepsTask = _api.GetRecipeStepsAsync(id);

        await Task.WhenAll(recipeTask, ingredientsTask, stepsTask);

        var recipeResult = recipeTask.Result;
        if (!recipeResult.IsSuccess || recipeResult.Data?.Recipe == null)
            return null;

        var api = recipeResult.Data.Recipe;
        var recipe = new Recipe
        {
            Id = api.Id,
            Title = api.Title,
            Description = api.Description ?? string.Empty,
            ImageUrl = ResolveImageUrl(api.ImageUrl),
            Difficulty = CapitalizeFirst(api.Difficulty),
            TimeMinutes = api.PreparationTime,
            Rating = api.AverageRating,
            CreatedAt = api.CreatedAt,
            AuthorId = api.Author?.Id ?? 0,
            AuthorUsername = api.Author?.Username ?? string.Empty,
            AuthorProfileImageUrl = ResolveImageUrl(api.Author?.ProfileImageUrl),
        };

        // Ingredients
        if (ingredientsTask.Result.IsSuccess && ingredientsTask.Result.Data != null)
        {
            recipe.Ingredients = ingredientsTask.Result.Data.Ingredients
                .Select(i =>
                {
                    double? qty = double.TryParse(i.Quantity,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                        ? parsed
                        : null;

                    return new Ingredient
                    {
                        Name = i.Name,
                        Quantity = qty,
                        Unit = i.Unit ?? string.Empty
                    };
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

        // Fetch all pages of comments
        var allApiComments = new List<ApiComment>();
        int page = 1;
        const int pageSize = 50;
        while (true)
        {
            var userCommentsResult = await _api.GetUserCommentsAsync(userId, page, pageSize);

            if (!userCommentsResult.IsSuccess || userCommentsResult.Data == null)
                break;

            var fetched = userCommentsResult.Data.Comments;
            if (fetched.Count == 0)
                break;

            allApiComments.AddRange(fetched);

            // Stop if we got fewer than requested (last page) or no pagination info
            var pagination = userCommentsResult.Data.Pagination;
            if (fetched.Count < pageSize || (pagination != null && page >= pagination.TotalPages))
                break;

            page++;
        }

        if (allApiComments.Count == 0)
            return comments;

        // Collect recipe IDs that we need to look up
        var apiComments = allApiComments;
        var recipeIdsToFetch = apiComments
            .Where(c => (c.RecipeId ?? 0) > 0 && c.Recipe == null)
            .Select(c => c.RecipeId!.Value)
            .Distinct()
            .ToList();

        // Build a lookup from embedded recipe data first
        var recipeLookup = new Dictionary<int, ApiRecipe>();
        foreach (var c in apiComments.Where(c => c.Recipe != null))
            recipeLookup[c.Recipe!.Id] = c.Recipe;

        // Fetch any missing recipe info in parallel
        if (recipeIdsToFetch.Count > 0)
        {
            var recipesResult = await _api.GetRecipesAsync(1, 100);
            if (recipesResult.IsSuccess && recipesResult.Data != null)
            {
                foreach (var r in recipesResult.Data.Recipes)
                    recipeLookup.TryAdd(r.Id, r);
            }
        }

        foreach (var c in apiComments)
        {
            var recipeId = c.RecipeId ?? 0;
            var recipeTitle = "Unknown Recipe";
            var authorUsername = "Unknown";
            var authorImageUrl = string.Empty;

            if (c.Recipe != null)
            {
                recipeId = c.Recipe.Id;
                recipeTitle = c.Recipe.Title;
                authorUsername = c.Recipe.Author?.Username ?? "Unknown";
                authorImageUrl = ResolveImageUrl(c.Recipe.Author?.ProfileImageUrl);
            }
            else if (recipeId > 0 && recipeLookup.TryGetValue(recipeId, out var recipe))
            {
                recipeTitle = recipe.Title;
                authorUsername = recipe.Author?.Username ?? "Unknown";
                authorImageUrl = ResolveImageUrl(recipe.Author?.ProfileImageUrl);
            }

            comments.Add(new UserComment
            {
                RecipeId = recipeId,
                RecipeTitle = recipeTitle,
                RecipeAuthorUsername = authorUsername,
                RecipeAuthorProfileImageUrl = authorImageUrl,
                CommentText = c.Content
            });
        }

        return comments;
    }

    public async Task<(bool Success, int? RecipeId, string? Error)> CreateRecipeAsync(
        string title, string description, string difficulty, int preparationTime,
        List<Ingredient> ingredients, List<RecipeStep> steps,
        string? imagePath,
        List<int>? categoryIds = null,
        List<int>? tagIds = null)
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
                PreparationTime = preparationTime,
                CategoryIds = categoryIds,
                TagIds = tagIds
            };

            var createResult = await _api.CreateRecipeAsync(request, imageStream, imageFileName);

            if (!createResult.IsSuccess || createResult.Data?.Recipe == null)
                return (false, null, createResult.ErrorMessage ?? "Failed to create recipe.");

            var recipeId = createResult.Data.Recipe.Id;

            // 2. Add ingredients one by one
            foreach (var ing in ingredients)
            {
                var qtyStr = ing.Quantity.HasValue
                    ? ing.Quantity.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : "0";

                await _api.AddRecipeIngredientAsync(recipeId, new ApiIngredient
                {
                    Name = ing.Name,
                    Quantity = qtyStr,
                    Unit = string.IsNullOrWhiteSpace(ing.Unit) ? "pcs" : ing.Unit
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

    public async Task<List<Comment>> GetCommentsByRecipeIdAsync(int recipeId, int? currentUserId = null)
    {
        var result = await _api.GetRecipeCommentsAsync(recipeId, 1, 100);

        if (!result.IsSuccess || result.Data == null)
            return new List<Comment>();

        return result.Data.Comments.Select(c => new Comment
        {
            Id = c.Id,
            UserId = c.User?.Id ?? 0,
            Username = c.User?.Username ?? "Unknown",
            ProfileImageUrl = ResolveImageUrl(c.User?.ProfileImageUrl),
            CreatedAt = c.CreatedAt ?? DateTime.MinValue,
            Text = c.Content,
            IsOwnComment = currentUserId.HasValue && c.User?.Id == currentUserId.Value
        }).ToList();
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var result = await _api.DeleteCommentAsync(commentId);
        return result.IsSuccess;
    }

    public async Task<int> GetMyRatingAsync(int recipeId)
    {
        var result = await _api.GetMyRecipeRatingAsync(recipeId);
        if (result.IsSuccess && result.Data?.Rating != null)
            return result.Data.Rating.Rating;

        return 0;
    }

    public async Task<(bool Success, double? UpdatedAverageRating)> RateRecipeAsync(int recipeId, int rating)
    {
        var request = new CreateRatingRequest { Rating = rating };

        // The backend uses PUT /recipes/{id}/ratings/me for both create and update
        var result = await _api.RateRecipeAsync(recipeId, request);

        if (!result.IsSuccess)
            return (false, null);

        // Fetch the updated average rating after successful submission
        var average = await GetRecipeAverageRatingAsync(recipeId);
        return (true, average);
    }

    public async Task<double?> GetRecipeAverageRatingAsync(int recipeId)
    {
        var ratingsResult = await _api.GetRecipeRatingsAsync(recipeId, 1, 1);
        if (ratingsResult.IsSuccess && ratingsResult.Data?.Summary != null)
            return ratingsResult.Data.Summary.AverageRating;

        // Fallback to summary endpoint
        var summaryResult = await _api.GetRecipeSummaryAsync(recipeId);
        if (summaryResult.IsSuccess && summaryResult.Data?.Summary != null)
            return summaryResult.Data.Summary.AverageRating;

        return null;
    }

    public async Task<Comment?> CreateCommentAsync(int recipeId, string text)
    {
        var request = new CreateCommentRequest { Content = text };
        var result = await _api.AddRecipeCommentAsync(recipeId, request);

        if (!result.IsSuccess)
            return null;

        // Use cached user from AuthService instead of an extra API call
        var currentUser = _authService.CurrentUser;

        // Re-fetch comments to get the server-assigned ID for the new comment
        var commentsResult = await _api.GetRecipeCommentsAsync(recipeId, 1, 100);
        var newComment = commentsResult.Data?.Comments
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault(c => c.User?.Id == currentUser?.Id && c.Content == text);

        return new Comment
        {
            Id = newComment?.Id ?? 0,
            UserId = currentUser?.Id ?? 0,
            Username = currentUser?.DisplayName ?? "You",
            ProfileImageUrl = currentUser?.AvatarUrl ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            Text = text,
            IsOwnComment = true
        };
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
        AuthorId = api.Author?.Id ?? 0,
        AuthorUsername = api.Author?.Username ?? string.Empty,
        AuthorProfileImageUrl = ResolveImageUrl(api.Author?.ProfileImageUrl),
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
