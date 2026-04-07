using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _authToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    // ?? Token management ?????????????????????????????????????????

    public void SetAuthToken(string? token)
    {
        _authToken = token;
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        else
            _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public string? GetAuthToken() => _authToken;

    // ?? Health ???????????????????????????????????????????????????

    public Task<ApiResult<HealthResponse>> GetHealthAsync() =>
        GetAsync<HealthResponse>("health");

    // ?? Auth ?????????????????????????????????????????????????????

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request) =>
        PostAsync<AuthResponse>("auth/login", request);

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request) =>
        PostAsync<AuthResponse>("auth/register", request);

    public Task<ApiResult<MessageResponse>> LogoutAsync() =>
        PostAsync<MessageResponse>("auth/logout", null);

    public Task<ApiResult<MeResponse>> GetMeAsync() =>
        GetAsync<MeResponse>("auth/me");

    public Task<ApiResult<MessageResponse>> ChangePasswordAsync(ChangePasswordRequest request) =>
        PostAsync<MessageResponse>("auth/change-password", request);

    public Task<ApiResult<MessageResponse>> ForgotPasswordAsync(ForgotPasswordRequest request) =>
        PostAsync<MessageResponse>("auth/forgot-password", request);

    public Task<ApiResult<MessageResponse>> ResetPasswordAsync(ResetPasswordRequest request) =>
        PostAsync<MessageResponse>("auth/reset-password", request);

    // ?? Recipes ??????????????????????????????????????????????????

    public Task<ApiResult<RecipesResponse>> GetRecipesAsync(
        int page = 1, int limit = 20,
        string? search = null, string? difficulty = null,
        string? sort = null, string? order = null)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"limit={limit}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(difficulty))
            queryParams.Add($"difficulty={Uri.EscapeDataString(difficulty)}");
        if (!string.IsNullOrWhiteSpace(sort))
            queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
        if (!string.IsNullOrWhiteSpace(order))
            queryParams.Add($"order={Uri.EscapeDataString(order)}");

        var url = $"recipes?{string.Join("&", queryParams)}";
        return GetAsync<RecipesResponse>(url);
    }

    public Task<ApiResult<RecipeSummaryResponse>> GetRecipeSummaryAsync(int recipeId) =>
        GetAsync<RecipeSummaryResponse>($"recipes/{recipeId}/summary");

    public Task<ApiResult<IngredientsResponse>> GetRecipeIngredientsAsync(int recipeId) =>
        GetAsync<IngredientsResponse>($"recipes/{recipeId}/ingredients");

    public Task<ApiResult<MessageResponse>> AddRecipeIngredientAsync(int recipeId, ApiIngredient ingredient) =>
        PostAsync<MessageResponse>($"recipes/{recipeId}/ingredients", ingredient);

    public Task<ApiResult<MessageResponse>> UpdateRecipeIngredientAsync(int recipeId, int ingredientId, ApiIngredient ingredient) =>
        PutAsync<MessageResponse>($"recipes/{recipeId}/ingredients/{ingredientId}", ingredient);

    public Task<ApiResult<MessageResponse>> DeleteRecipeIngredientAsync(int recipeId, int ingredientId) =>
        DeleteAsync<MessageResponse>($"recipes/{recipeId}/ingredients/{ingredientId}");

    public Task<ApiResult<StepsResponse>> GetRecipeStepsAsync(int recipeId) =>
        GetAsync<StepsResponse>($"recipes/{recipeId}/steps");

    public Task<ApiResult<MessageResponse>> AddRecipeStepAsync(int recipeId, ApiStep step) =>
        PostAsync<MessageResponse>($"recipes/{recipeId}/steps", step);

    public Task<ApiResult<MessageResponse>> UpdateRecipeStepAsync(int recipeId, int stepId, ApiStep step) =>
        PutAsync<MessageResponse>($"recipes/{recipeId}/steps/{stepId}", step);

    public Task<ApiResult<MessageResponse>> DeleteRecipeStepAsync(int recipeId, int stepId) =>
        DeleteAsync<MessageResponse>($"recipes/{recipeId}/steps/{stepId}");

    public Task<ApiResult<MessageResponse>> ReorderRecipeStepsAsync(int recipeId, ReorderStepsRequest request) =>
        PutAsync<MessageResponse>($"recipes/{recipeId}/steps/reorder", request);

    public Task<ApiResult<CategoriesResponse>> GetRecipeCategoriesAsync(int recipeId) =>
        GetAsync<CategoriesResponse>($"recipes/{recipeId}/categories");

    public Task<ApiResult<TagsResponse>> GetRecipeTagsAsync(int recipeId) =>
        GetAsync<TagsResponse>($"recipes/{recipeId}/tags");

    public Task<ApiResult<CommentsResponse>> GetRecipeCommentsAsync(int recipeId, int page = 1, int limit = 20) =>
        GetAsync<CommentsResponse>($"recipes/{recipeId}/comments?page={page}&limit={limit}");

    public Task<ApiResult<MessageResponse>> AddRecipeCommentAsync(int recipeId, CreateCommentRequest request) =>
        PostAsync<MessageResponse>($"recipes/{recipeId}/comments", request);

    public Task<ApiResult<RatingsResponse>> GetRecipeRatingsAsync(int recipeId, int page = 1, int limit = 20) =>
        GetAsync<RatingsResponse>($"recipes/{recipeId}/ratings?page={page}&limit={limit}");

    public Task<ApiResult<MyRatingResponse>> GetMyRecipeRatingAsync(int recipeId) =>
        GetAsync<MyRatingResponse>($"recipes/{recipeId}/ratings/me");

    public Task<ApiResult<MessageResponse>> RateRecipeAsync(int recipeId, CreateRatingRequest request) =>
        PostAsync<MessageResponse>($"recipes/{recipeId}/ratings", request);

    public Task<ApiResult<FavoriteResponse>> AddRecipeFavoriteAsync(int recipeId) =>
        PostAsync<FavoriteResponse>($"recipes/{recipeId}/favorite", null);

    public Task<ApiResult<MessageResponse>> RemoveRecipeFavoriteAsync(int recipeId) =>
        DeleteAsync<MessageResponse>($"recipes/{recipeId}/favorite");

    public async Task<ApiResult<MessageResponse>> UploadRecipeImageAsync(int recipeId, Stream imageStream, string fileName)
    {
        return await UploadAsync<MessageResponse>($"recipes/{recipeId}/image", imageStream, fileName);
    }

    // ?? Categories ???????????????????????????????????????????????

    public Task<ApiResult<CategoriesResponse>> GetCategoriesAsync() =>
        GetAsync<CategoriesResponse>("categories");

    public Task<ApiResult<CategoryResponse>> GetCategoryAsync(int categoryId) =>
        GetAsync<CategoryResponse>($"categories/{categoryId}");

    // ?? Tags ?????????????????????????????????????????????????????

    public Task<ApiResult<TagsResponse>> GetTagsAsync() =>
        GetAsync<TagsResponse>("tags");

    public Task<ApiResult<TagResponse>> GetTagAsync(int tagId) =>
        GetAsync<TagResponse>($"tags/{tagId}");

    // ?? Comments ?????????????????????????????????????????????????

    public Task<ApiResult<MessageResponse>> UpdateCommentAsync(int commentId, CreateCommentRequest request) =>
        PutAsync<MessageResponse>($"comments/{commentId}", request);

    public Task<ApiResult<MessageResponse>> DeleteCommentAsync(int commentId) =>
        DeleteAsync<MessageResponse>($"comments/{commentId}");

    // ?? Meta ?????????????????????????????????????????????????????

    public Task<ApiResult<DifficultiesResponse>> GetDifficultiesAsync() =>
        GetAsync<DifficultiesResponse>("meta/difficulties");

    public Task<ApiResult<RolesResponse>> GetRolesAsync() =>
        GetAsync<RolesResponse>("meta/roles");

    // ?? Recipe Books ?????????????????????????????????????????????

    public Task<ApiResult<RecipeBooksResponse>> GetRecipeBooksAsync(int page = 1, int limit = 20) =>
        GetAsync<RecipeBooksResponse>($"recipe-books?page={page}&limit={limit}");

    public Task<ApiResult<MessageResponse>> CloneRecipeBookAsync(int bookId) =>
        PostAsync<MessageResponse>($"recipe-books/{bookId}/clone", null);

    public Task<ApiResult<MessageResponse>> AddRecipeToBookAsync(int bookId, int recipeId) =>
        PostAsync<MessageResponse>($"recipe-books/{bookId}/recipes/{recipeId}", null);

    public Task<ApiResult<MessageResponse>> RemoveRecipeFromBookAsync(int bookId, int recipeId) =>
        DeleteAsync<MessageResponse>($"recipe-books/{bookId}/recipes/{recipeId}");

    // ?? Users ????????????????????????????????????????????????????

    public Task<ApiResult<UserResponse>> GetUserAsync(int userId) =>
        GetAsync<UserResponse>($"users/{userId}");

    public Task<ApiResult<RecipesResponse>> GetUserRecipesAsync(int userId, int page = 1, int limit = 20) =>
        GetAsync<RecipesResponse>($"users/{userId}/recipes?page={page}&limit={limit}");

    public Task<ApiResult<CommentsResponse>> GetUserCommentsAsync(int userId, int page = 1, int limit = 20) =>
        GetAsync<CommentsResponse>($"users/{userId}/comments?page={page}&limit={limit}");

    public Task<ApiResult<RatingsResponse>> GetUserRatingsAsync(int userId, int page = 1, int limit = 20) =>
        GetAsync<RatingsResponse>($"users/{userId}/ratings?page={page}&limit={limit}");

    public Task<ApiResult<RecipeBooksResponse>> GetUserRecipeBooksAsync(int userId, int page = 1, int limit = 20) =>
        GetAsync<RecipeBooksResponse>($"users/{userId}/recipe-books?page={page}&limit={limit}");

    public Task<ApiResult<UserStatsResponse>> GetUserStatsAsync(int userId) =>
        GetAsync<UserStatsResponse>($"users/{userId}/stats");

    // ?? Users/me ?????????????????????????????????????????????????

    public Task<ApiResult<FavoritesResponse>> GetMyFavoritesAsync(int page = 1, int limit = 20) =>
        GetAsync<FavoritesResponse>($"users/me/favorites?page={page}&limit={limit}");

    public Task<ApiResult<UserStatsResponse>> GetMyStatsAsync() =>
        GetAsync<UserStatsResponse>("users/me/stats");

    public Task<ApiResult<MessageResponse>> UpdateMyProfileAsync(UpdateProfileRequest request) =>
        PatchAsync<MessageResponse>("users/me", request);

    public async Task<ApiResult<UploadResponse>> UploadProfileImageAsync(Stream imageStream, string fileName)
    {
        return await UploadAsync<UploadResponse>("uploads", imageStream, fileName);
    }

    public Task<ApiResult<MessageResponse>> DeactivateMyAccountAsync() =>
        PostAsync<MessageResponse>("users/me/deactivate", null);

    // ?? Reports ??????????????????????????????????????????????????

    public Task<ApiResult<MessageResponse>> CreateReportAsync(CreateReportRequest request) =>
        PostAsync<MessageResponse>("reports", request);

    // ?? Search ???????????????????????????????????????????????????

    public Task<ApiResult<SearchSuggestionsResponse>> GetSearchSuggestionsAsync(string query) =>
        GetAsync<SearchSuggestionsResponse>($"search/suggestions?q={Uri.EscapeDataString(query)}");

    // ?? Uploads ??????????????????????????????????????????????????

    public Task<ApiResult<UploadResponse>> UploadFileAsync(Stream fileStream, string fileName) =>
        UploadAsync<UploadResponse>("uploads", fileStream, fileName);

    // ??????????????????????????????????????????????????????????????
    //  Private HTTP helpers
    // ??????????????????????????????????????????????????????????????

    private async Task<ApiResult<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> PostAsync<T>(string endpoint, object? body)
    {
        try
        {
            var json = body != null
                ? JsonSerializer.Serialize(body, _jsonOptions)
                : "{}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> PutAsync<T>(string endpoint, object? body)
    {
        try
        {
            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PutAsync(endpoint, content);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> PatchAsync<T>(string endpoint, object? body)
    {
        try
        {
            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> UploadAsync<T>(string endpoint, Stream fileStream, string fileName)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            var contentType = GetMimeType(fileName);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formData.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync(endpoint, formData);
            return await HandleResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            ".tiff" or ".tif" => "image/tiff",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    private async Task<ApiResult<T>> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                return ApiResult<T>.Success(default!, statusCode);

            try
            {
                var data = JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);
                return data != null
                    ? ApiResult<T>.Success(data, statusCode)
                    : ApiResult<T>.Success(default!, statusCode);
            }
            catch (JsonException ex)
            {
                return ApiResult<T>.Failure($"Failed to parse response: {ex.Message}", statusCode);
            }
        }

        // Try to extract an error message from the response body
        var errorMessage = $"Request failed with status {statusCode}.";
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var errorProp))
                errorMessage = errorProp.GetString() ?? errorMessage;
            else if (doc.RootElement.TryGetProperty("message", out var msgProp))
                errorMessage = msgProp.GetString() ?? errorMessage;
        }
        catch
        {
            // Couldn't parse error body — use default message
        }

        return ApiResult<T>.Failure(errorMessage, statusCode);
    }
}
