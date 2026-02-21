namespace RealChat.Api.Models;

/// <summary>
/// JSON message structure for real-time chat (type, senderId, receiverId, data).
/// </summary>
public class ChatMessageDto
{
    public string Type { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public string? ReceiverId { get; set; }
    public string? Data { get; set; }
}
