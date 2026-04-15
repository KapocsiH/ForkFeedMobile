namespace ForkFeedMobile.Models;

public class ShoppingListItem
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
}
