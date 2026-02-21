namespace RealChat.Api.Models;

/// <summary>
/// Persisted chat message.
/// </summary>
public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
