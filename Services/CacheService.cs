using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class CacheService
{
    private List<ApiCategory>? _categories;
    private List<ApiTag>? _tags;
    private DateTime _categoriesCachedAt;
    private DateTime _tagsCachedAt;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    private readonly IApiService _api;

    public CacheService(IApiService api)
    {
        _api = api;
    }

    public async Task<List<ApiCategory>> GetCategoriesAsync()
    {
        if (_categories != null && DateTime.UtcNow - _categoriesCachedAt < _cacheDuration)
            return _categories;

        var result = await _api.GetCategoriesAsync();
        if (result.IsSuccess && result.Data?.Categories != null)
        {
            _categories = result.Data.Categories.ToList();
            _categoriesCachedAt = DateTime.UtcNow;
            return _categories;
        }

        return _categories ?? new List<ApiCategory>();
    }

    public async Task<List<ApiTag>> GetTagsAsync()
    {
        if (_tags != null && DateTime.UtcNow - _tagsCachedAt < _cacheDuration)
            return _tags;

        var result = await _api.GetTagsAsync();
        if (result.IsSuccess && result.Data?.Tags != null)
        {
            _tags = result.Data.Tags.ToList();
            _tagsCachedAt = DateTime.UtcNow;
            return _tags;
        }

        return _tags ?? new List<ApiTag>();
    }

    public void Invalidate()
    {
        _categories = null;
        _tags = null;
    }
}
