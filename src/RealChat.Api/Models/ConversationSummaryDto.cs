namespace RealChat.Api.Models;

/// <summary>
/// Summary of a conversation for the recent-chats list.
/// </summary>
public class ConversationSummaryDto
{
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
}
