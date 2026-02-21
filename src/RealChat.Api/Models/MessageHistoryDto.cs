namespace RealChat.Api.Models;

/// <summary>
/// A message in chat history API response.
/// </summary>
public class MessageHistoryDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
