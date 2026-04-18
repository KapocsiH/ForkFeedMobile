namespace ForkFeedMobile.Models;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsOwnComment { get; set; }
}
