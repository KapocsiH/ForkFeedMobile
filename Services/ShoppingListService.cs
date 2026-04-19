using System.Text.Json;
using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class ShoppingListService
{
    private const string PreferenceKeyPrefix = "shopping_list_";
    public Task<List<ShoppingListItem>> LoadAsync(int userId)
    {
        var key = GetKey(userId);
        var json = Preferences.Default.Get(key, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new List<ShoppingListItem>());

        try
        {
            var items = JsonSerializer.Deserialize<List<ShoppingListItem>>(json);
            return Task.FromResult(items ?? new List<ShoppingListItem>());
        }
        catch
        {
            return Task.FromResult(new List<ShoppingListItem>());
        }
    }
    public Task SaveAsync(int userId, List<ShoppingListItem> items)
    {
        var key = GetKey(userId);
        var json = JsonSerializer.Serialize(items);
        Preferences.Default.Set(key, json);
        return Task.CompletedTask;
    }
    public async Task AddIngredientsAsync(int userId, IEnumerable<Ingredient> ingredients)
    {
        var items = await LoadAsync(userId);

        foreach (var ingredient in ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredient.Name))
                continue;

            var qty = ingredient.Quantity.GetValueOrDefault();
            var unit = ingredient.Unit ?? string.Empty;

            var existing = items.FirstOrDefault(i =>
                string.Equals(i.Name, ingredient.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(i.Unit, unit, StringComparison.OrdinalIgnoreCase));

            if (existing != null && qty > 0 && existing.Quantity > 0)
            {
                existing.Quantity += qty;
            }
            else if (existing == null)
            {
                items.Add(new ShoppingListItem
                {
                    Name = ingredient.Name,
                    Quantity = qty,
                    Unit = unit
                });
            }
        }

        await SaveAsync(userId, items);
    }
    public async Task RemoveItemAsync(int userId, ShoppingListItem item)
    {
        var items = await LoadAsync(userId);
        var match = items.FirstOrDefault(i =>
            string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(i.Unit, item.Unit, StringComparison.OrdinalIgnoreCase) &&
            Math.Abs(i.Quantity - item.Quantity) < 0.001);

        if (match != null)
        {
            items.Remove(match);
            await SaveAsync(userId, items);
        }
    }
    public async Task ClearAsync(int userId)
    {
        await SaveAsync(userId, new List<ShoppingListItem>());
    }

    private static string GetKey(int userId) => $"{PreferenceKeyPrefix}{userId}";
}
