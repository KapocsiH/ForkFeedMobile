using System.Text.Json.Serialization;

namespace ForkFeedMobile.Models;

// ?? Generic wrapper ??????????????????????????????????????????????

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult<T> Success(T data, int statusCode = 200) =>
        new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static ApiResult<T> Failure(string error, int statusCode = 0) =>
        new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
}

// ?? Pagination ???????????????????????????????????????????????????

public class PaginationInfo
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}

// ?? Health ???????????????????????????????????????????????????????

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("db")]
    public string Db { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

// ?? Auth ?????????????????????????????????????????????????????????

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [JsonPropertyName("current_password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("user")]
    public ApiUser? User { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class MeResponse
{
    [JsonPropertyName("user")]
    public ApiUser? User { get; set; }
}

// ?? User ?????????????????????????????????????????????????????????

public class ApiUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}

public class UserResponse
{
    [JsonPropertyName("user")]
    public ApiUser? User { get; set; }
}

public class UserStatsResponse
{
    [JsonPropertyName("stats")]
    public UserStats? Stats { get; set; }
}

public class UserStats
{
    [JsonPropertyName("recipes_count")]
    public int RecipesCount { get; set; }

    [JsonPropertyName("recipe_books_count")]
    public int RecipeBooksCount { get; set; }

    [JsonPropertyName("average_recipe_rating")]
    public double AverageRecipeRating { get; set; }
}

// ?? Recipe ???????????????????????????????????????????????????????

public class ApiRecipe
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("preparation_time")]
    public int PreparationTime { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "easy";

    [JsonPropertyName("average_rating")]
    public double AverageRating { get; set; }

    [JsonPropertyName("rating_count")]
    public int RatingCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("author")]
    public ApiUser? Author { get; set; }

    [JsonPropertyName("categories")]
    public List<ApiCategory>? Categories { get; set; }

    [JsonPropertyName("tags")]
    public List<ApiTag>? Tags { get; set; }
}

public class RecipesResponse
{
    [JsonPropertyName("recipes")]
    public List<ApiRecipe> Recipes { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class RecipeSummaryResponse
{
    [JsonPropertyName("summary")]
    public ApiRecipeSummary? Summary { get; set; }
}

public class ApiRecipeSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("average_rating")]
    public double AverageRating { get; set; }

    [JsonPropertyName("rating_count")]
    public int RatingCount { get; set; }

    [JsonPropertyName("preparation_time")]
    public int PreparationTime { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
}

// ?? Ingredient ???????????????????????????????????????????????????

public class ApiIngredient
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}

public class IngredientsResponse
{
    [JsonPropertyName("ingredients")]
    public List<ApiIngredient> Ingredients { get; set; } = new();
}

// ?? Step ?????????????????????????????????????????????????????????

public class ApiStep
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("step_number")]
    public int StepNumber { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class StepsResponse
{
    [JsonPropertyName("steps")]
    public List<ApiStep> Steps { get; set; } = new();
}

public class ReorderStepsRequest
{
    [JsonPropertyName("step_ids")]
    public List<int> StepIds { get; set; } = new();
}

// ?? Category ?????????????????????????????????????????????????????

public class ApiCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}

public class CategoriesResponse
{
    [JsonPropertyName("categories")]
    public List<ApiCategory> Categories { get; set; } = new();
}

public class CategoryResponse
{
    [JsonPropertyName("category")]
    public ApiCategory? Category { get; set; }
}

// ?? Tag ??????????????????????????????????????????????????????????

public class ApiTag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}

public class TagsResponse
{
    [JsonPropertyName("tags")]
    public List<ApiTag> Tags { get; set; } = new();
}

public class TagResponse
{
    [JsonPropertyName("tag")]
    public ApiTag? Tag { get; set; }
}

// ?? Comment ??????????????????????????????????????????????????????

public class ApiComment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("user")]
    public ApiUser? User { get; set; }
}

public class CommentsResponse
{
    [JsonPropertyName("comments")]
    public List<ApiComment> Comments { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class CreateCommentRequest
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

// ?? Rating ???????????????????????????????????????????????????????

public class ApiRating
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("user")]
    public ApiUser? User { get; set; }
}

public class RatingsResponse
{
    [JsonPropertyName("ratings")]
    public List<ApiRating> Ratings { get; set; } = new();

    [JsonPropertyName("summary")]
    public RatingSummary? Summary { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class RatingSummary
{
    [JsonPropertyName("average_rating")]
    public double AverageRating { get; set; }

    [JsonPropertyName("rating_count")]
    public int RatingCount { get; set; }
}

public class CreateRatingRequest
{
    [JsonPropertyName("rating")]
    public int Rating { get; set; }
}

public class MyRatingResponse
{
    [JsonPropertyName("rating")]
    public ApiRating? Rating { get; set; }
}

// ?? Favorite ?????????????????????????????????????????????????????

public class FavoriteResponse
{
    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class FavoritesResponse
{
    [JsonPropertyName("recipes")]
    public List<ApiRecipe> Recipes { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

// ?? Recipe Book ??????????????????????????????????????????????????

public class ApiRecipeBook
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_public")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("owner")]
    public ApiUser? Owner { get; set; }

    [JsonPropertyName("recipe_count")]
    public int RecipeCount { get; set; }
}

public class RecipeBooksResponse
{
    [JsonPropertyName("recipe_books")]
    public List<ApiRecipeBook> RecipeBooks { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

// ?? Report ???????????????????????????????????????????????????????

public class CreateReportRequest
{
    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = string.Empty;

    [JsonPropertyName("target_id")]
    public int TargetId { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

// ?? Search ???????????????????????????????????????????????????????

public class SearchSuggestionsResponse
{
    [JsonPropertyName("recipes")]
    public List<SearchSuggestionItem> Recipes { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<SearchSuggestionItem> Categories { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<SearchSuggestionItem> Tags { get; set; } = new();
}

public class SearchSuggestionItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

// ?? Meta ?????????????????????????????????????????????????????????

public class DifficultiesResponse
{
    [JsonPropertyName("difficulties")]
    public List<string> Difficulties { get; set; } = new();
}

public class RolesResponse
{
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

// ?? Upload ???????????????????????????????????????????????????????

public class UploadResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

// ?? Generic message response ?????????????????????????????????????

public class MessageResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
