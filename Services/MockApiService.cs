using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

/// <summary>
/// Mock implementation of IApiService for offline development and testing.
/// Swap this in via MauiProgram.cs instead of ApiService.
/// </summary>
public class MockApiService : IApiService
{
    private readonly List<ApiRecipe> _recipes;
    private readonly HashSet<int> _favoriteIds = new();

    public MockApiService()
    {
        _recipes = GenerateMockRecipes();
    }

    public Task<ApiResult<HealthResponse>> GetHealthAsync() =>
        Ok(new HealthResponse { Status = "ok", Db = "ok", Timestamp = DateTime.UtcNow.ToString("o") });

    // ?? Auth ?????????????????????????????????????????????????????

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request) =>
        Ok(new AuthResponse
        {
            Token = "mock-token",
            User = new ApiUser { Id = 1, Username = "MockChef", Email = request.Email }
        });

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request) =>
        Ok(new AuthResponse
        {
            Token = "mock-token",
            User = new ApiUser { Id = 1, Username = request.Username, Email = request.Email }
        });

    public Task<ApiResult<MessageResponse>> LogoutAsync() =>
        Ok(new MessageResponse { Message = "Logged out" });

    public Task<ApiResult<MeResponse>> GetMeAsync() =>
        Ok(new MeResponse
        {
            User = new ApiUser
            {
                Id = 1,
                Username = "MockChef",
                Email = "mock@chef.com",
                ProfileImageUrl = "https://i.pravatar.cc/150?img=12",
                CreatedAt = new DateTime(2023, 3, 15)
            }
        });

    public Task<ApiResult<MessageResponse>> ChangePasswordAsync(ChangePasswordRequest request) =>
        Ok(new MessageResponse { Message = "Password changed" });

    public Task<ApiResult<MessageResponse>> ForgotPasswordAsync(ForgotPasswordRequest request) =>
        Ok(new MessageResponse { Message = "Reset link sent" });

    public Task<ApiResult<MessageResponse>> ResetPasswordAsync(ResetPasswordRequest request) =>
        Ok(new MessageResponse { Message = "Password reset" });

    // ?? Recipes ??????????????????????????????????????????????????

    public Task<ApiResult<RecipesResponse>> GetRecipesAsync(
        int page = 1, int limit = 20,
        string? search = null, string? difficulty = null,
        string? sort = null, string? order = null)
    {
        IEnumerable<ApiRecipe> query = _recipes;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(difficulty) && !difficulty.Equals("all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => r.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));

        var total = query.Count();
        var items = query.Skip((page - 1) * limit).Take(limit).ToList();

        return Ok(new RecipesResponse
        {
            Recipes = items,
            Pagination = new PaginationInfo
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            }
        });
    }

    public Task<ApiResult<RecipeSummaryResponse>> GetRecipeSummaryAsync(int recipeId)
    {
        var r = _recipes.FirstOrDefault(x => x.Id == recipeId);
        if (r == null) return Fail<RecipeSummaryResponse>("Not found", 404);
        return Ok(new RecipeSummaryResponse
        {
            Summary = new ApiRecipeSummary
            {
                Id = r.Id, Title = r.Title,
                AverageRating = r.AverageRating, RatingCount = r.RatingCount,
                PreparationTime = r.PreparationTime, Difficulty = r.Difficulty
            }
        });
    }

    public Task<ApiResult<IngredientsResponse>> GetRecipeIngredientsAsync(int recipeId) =>
        Ok(new IngredientsResponse
        {
            Ingredients = new()
            {
                new() { Id = 1, Name = "Mock ingredient 1", Quantity = 1, Unit = "cup" },
                new() { Id = 2, Name = "Mock ingredient 2", Quantity = 200, Unit = "g" }
            }
        });

    public Task<ApiResult<MessageResponse>> AddRecipeIngredientAsync(int recipeId, ApiIngredient ingredient) =>
        Ok(new MessageResponse { Message = "Ingredient added" });

    public Task<ApiResult<MessageResponse>> UpdateRecipeIngredientAsync(int recipeId, int ingredientId, ApiIngredient ingredient) =>
        Ok(new MessageResponse { Message = "Ingredient updated" });

    public Task<ApiResult<MessageResponse>> DeleteRecipeIngredientAsync(int recipeId, int ingredientId) =>
        Ok(new MessageResponse { Message = "Ingredient deleted" });

    public Task<ApiResult<StepsResponse>> GetRecipeStepsAsync(int recipeId) =>
        Ok(new StepsResponse
        {
            Steps = new()
            {
                new() { Id = 1, StepNumber = 1, Description = "Mock step 1" },
                new() { Id = 2, StepNumber = 2, Description = "Mock step 2" }
            }
        });

    public Task<ApiResult<MessageResponse>> AddRecipeStepAsync(int recipeId, ApiStep step) =>
        Ok(new MessageResponse { Message = "Step added" });

    public Task<ApiResult<MessageResponse>> UpdateRecipeStepAsync(int recipeId, int stepId, ApiStep step) =>
        Ok(new MessageResponse { Message = "Step updated" });

    public Task<ApiResult<MessageResponse>> DeleteRecipeStepAsync(int recipeId, int stepId) =>
        Ok(new MessageResponse { Message = "Step deleted" });

    public Task<ApiResult<MessageResponse>> ReorderRecipeStepsAsync(int recipeId, ReorderStepsRequest request) =>
        Ok(new MessageResponse { Message = "Steps reordered" });

    public Task<ApiResult<CategoriesResponse>> GetRecipeCategoriesAsync(int recipeId) =>
        Ok(new CategoriesResponse { Categories = new() { new() { Id = 1, Name = "Mock category" } } });

    public Task<ApiResult<TagsResponse>> GetRecipeTagsAsync(int recipeId) =>
        Ok(new TagsResponse { Tags = new() { new() { Id = 1, Name = "mock" } } });

    public Task<ApiResult<CommentsResponse>> GetRecipeCommentsAsync(int recipeId, int page = 1, int limit = 20) =>
        Ok(new CommentsResponse { Comments = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<MessageResponse>> AddRecipeCommentAsync(int recipeId, CreateCommentRequest request) =>
        Ok(new MessageResponse { Message = "Comment added" });

    public Task<ApiResult<RatingsResponse>> GetRecipeRatingsAsync(int recipeId, int page = 1, int limit = 20) =>
        Ok(new RatingsResponse
        {
            Ratings = new(),
            Summary = new RatingSummary { AverageRating = 4.5, RatingCount = 2 },
            Pagination = EmptyPagination(page, limit)
        });

    public Task<ApiResult<MyRatingResponse>> GetMyRecipeRatingAsync(int recipeId) =>
        Ok(new MyRatingResponse());

    public Task<ApiResult<MessageResponse>> RateRecipeAsync(int recipeId, CreateRatingRequest request) =>
        Ok(new MessageResponse { Message = "Rated" });

    public Task<ApiResult<FavoriteResponse>> AddRecipeFavoriteAsync(int recipeId)
    {
        _favoriteIds.Add(recipeId);
        return Ok(new FavoriteResponse { IsFavorite = true, Message = "Added to favorites" });
    }

    public Task<ApiResult<MessageResponse>> RemoveRecipeFavoriteAsync(int recipeId)
    {
        _favoriteIds.Remove(recipeId);
        return Ok(new MessageResponse { Message = "Removed from favorites" });
    }

    public Task<ApiResult<CreateRecipeResponse>> CreateRecipeAsync(CreateRecipeRequest request, Stream? imageStream = null, string? imageFileName = null)
    {
        var newId = _recipes.Count > 0 ? _recipes.Max(r => r.Id) + 1 : 1;
        var newRecipe = new ApiRecipe
        {
            Id = newId,
            Title = request.Title,
            Description = request.Description,
            PreparationTime = request.PreparationTime,
            Difficulty = request.Difficulty,
            ImageUrl = "https://images.unsplash.com/photo-1495521821757-a1efb6729352?w=600",
            CreatedAt = DateTime.UtcNow,
            Author = new ApiUser { Id = 1, Username = "MockChef" }
        };
        _recipes.Add(newRecipe);
        return Ok(new CreateRecipeResponse { Recipe = newRecipe, Message = "Recipe created" });
    }

    public Task<ApiResult<MessageResponse>> UploadRecipeImageAsync(int recipeId, Stream imageStream, string fileName) =>
        Ok(new MessageResponse { Message = "Image uploaded" });

    // ?? Categories ???????????????????????????????????????????????

    public Task<ApiResult<CategoriesResponse>> GetCategoriesAsync() =>
        Ok(new CategoriesResponse
        {
            Categories = new()
            {
                new() { Id = 1, Name = "Soup" },
                new() { Id = 2, Name = "Main course" },
                new() { Id = 3, Name = "Dessert" }
            }
        });

    public Task<ApiResult<CategoryResponse>> GetCategoryAsync(int categoryId) =>
        Ok(new CategoryResponse { Category = new() { Id = categoryId, Name = "Mock category" } });

    // ?? Tags ?????????????????????????????????????????????????????

    public Task<ApiResult<TagsResponse>> GetTagsAsync() =>
        Ok(new TagsResponse
        {
            Tags = new()
            {
                new() { Id = 1, Name = "hungarian" },
                new() { Id = 2, Name = "vegetarian" }
            }
        });

    public Task<ApiResult<TagResponse>> GetTagAsync(int tagId) =>
        Ok(new TagResponse { Tag = new() { Id = tagId, Name = "mock" } });

    // ?? Comments ?????????????????????????????????????????????????

    public Task<ApiResult<MessageResponse>> UpdateCommentAsync(int commentId, CreateCommentRequest request) =>
        Ok(new MessageResponse { Message = "Updated" });

    public Task<ApiResult<MessageResponse>> DeleteCommentAsync(int commentId) =>
        Ok(new MessageResponse { Message = "Deleted" });

    // ?? Meta ?????????????????????????????????????????????????????

    public Task<ApiResult<DifficultiesResponse>> GetDifficultiesAsync() =>
        Ok(new DifficultiesResponse { Difficulties = new() { "easy", "medium", "hard" } });

    public Task<ApiResult<RolesResponse>> GetRolesAsync() =>
        Ok(new RolesResponse { Roles = new() { "user", "admin" } });

    // ?? Recipe Books ?????????????????????????????????????????????

    public Task<ApiResult<RecipeBooksResponse>> GetRecipeBooksAsync(int page = 1, int limit = 20) =>
        Ok(new RecipeBooksResponse { RecipeBooks = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<MessageResponse>> CloneRecipeBookAsync(int bookId) =>
        Ok(new MessageResponse { Message = "Cloned" });

    public Task<ApiResult<MessageResponse>> AddRecipeToBookAsync(int bookId, int recipeId) =>
        Ok(new MessageResponse { Message = "Added" });

    public Task<ApiResult<MessageResponse>> RemoveRecipeFromBookAsync(int bookId, int recipeId) =>
        Ok(new MessageResponse { Message = "Removed" });

    // ?? Users ????????????????????????????????????????????????????

    public Task<ApiResult<UserResponse>> GetUserAsync(int userId) =>
        Ok(new UserResponse { User = new ApiUser { Id = userId, Username = "MockUser" } });

    public Task<ApiResult<RecipesResponse>> GetUserRecipesAsync(int userId, int page = 1, int limit = 20) =>
        Ok(new RecipesResponse { Recipes = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<CommentsResponse>> GetUserCommentsAsync(int userId, int page = 1, int limit = 20) =>
        Ok(new CommentsResponse { Comments = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<RatingsResponse>> GetUserRatingsAsync(int userId, int page = 1, int limit = 20) =>
        Ok(new RatingsResponse { Ratings = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<RecipeBooksResponse>> GetUserRecipeBooksAsync(int userId, int page = 1, int limit = 20) =>
        Ok(new RecipeBooksResponse { RecipeBooks = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<UserStatsResponse>> GetUserStatsAsync(int userId) =>
        Ok(new UserStatsResponse { Stats = new UserStats { RecipesCount = 5, RecipeBooksCount = 1, AverageRecipeRating = 4.2 } });

    // ?? Users/me ?????????????????????????????????????????????????

    public Task<ApiResult<FavoritesResponse>> GetMyFavoritesAsync(int page = 1, int limit = 20) =>
        Ok(new FavoritesResponse { Recipes = new(), Pagination = EmptyPagination(page, limit) });

    public Task<ApiResult<UserStatsResponse>> GetMyStatsAsync() =>
        Ok(new UserStatsResponse { Stats = new UserStats { RecipesCount = 5, RecipeBooksCount = 1, AverageRecipeRating = 4.2 } });

    public Task<ApiResult<MessageResponse>> UpdateMyProfileAsync(UpdateProfileRequest request) =>
        Ok(new MessageResponse { Message = "Profile updated" });

    public Task<ApiResult<UploadResponse>> UploadProfileImageAsync(Stream imageStream, string fileName) =>
        Ok(new UploadResponse { Url = "https://mock.url/profile.jpg", Message = "Profile image uploaded" });

    public Task<ApiResult<MessageResponse>> DeactivateMyAccountAsync() =>
        Ok(new MessageResponse { Message = "Deactivated" });

    // ?? Reports ??????????????????????????????????????????????????

    public Task<ApiResult<MessageResponse>> CreateReportAsync(CreateReportRequest request) =>
        Ok(new MessageResponse { Message = "Report created" });

    // ?? Search ???????????????????????????????????????????????????

    public Task<ApiResult<SearchSuggestionsResponse>> GetSearchSuggestionsAsync(string query) =>
        Ok(new SearchSuggestionsResponse
        {
            Recipes = _recipes
                .Where(r => r.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(r => new SearchSuggestionItem { Id = r.Id, Title = r.Title })
                .Take(5).ToList(),
            Categories = new(),
            Tags = new()
        });

    // ?? Uploads ??????????????????????????????????????????????????

    public Task<ApiResult<UploadResponse>> UploadFileAsync(Stream fileStream, string fileName) =>
        Ok(new UploadResponse { Url = "https://mock.url/image.jpg", Message = "Uploaded" });

    // ??????????????????????????????????????????????????????????????
    //  Helpers
    // ??????????????????????????????????????????????????????????????

    private static async Task<ApiResult<T>> Ok<T>(T data)
    {
        await Task.Delay(300); // simulate network latency
        return ApiResult<T>.Success(data);
    }

    private static async Task<ApiResult<T>> Fail<T>(string message, int code)
    {
        await Task.Delay(100);
        return ApiResult<T>.Failure(message, code);
    }

    private static PaginationInfo EmptyPagination(int page, int limit) =>
        new() { Page = page, Limit = limit, Total = 0, TotalPages = 0 };

    private static List<ApiRecipe> GenerateMockRecipes() => new()
    {
        new() { Id = 1, Title = "Classic Margherita Pizza", ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=600", PreparationTime = 30, Difficulty = "easy", AverageRating = 4.7, RatingCount = 10, CreatedAt = DateTime.Now.AddDays(-2) },
        new() { Id = 2, Title = "Chicken Tikka Masala", ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", PreparationTime = 50, Difficulty = "medium", AverageRating = 4.9, RatingCount = 8, CreatedAt = DateTime.Now.AddDays(-5) },
        new() { Id = 3, Title = "Beef Wellington", ImageUrl = "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600", PreparationTime = 120, Difficulty = "hard", AverageRating = 4.5, RatingCount = 5, CreatedAt = DateTime.Now.AddDays(-1) },
    };
}
