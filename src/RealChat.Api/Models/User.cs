namespace RealChat.Api.Models;

/// <summary>
/// Represents a chat user (for presence and conversation list).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
}
