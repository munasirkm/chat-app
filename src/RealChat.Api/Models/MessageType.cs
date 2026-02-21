namespace RealChat.Api.Models;

/// <summary>
/// Wire format message type (connect, chat, typing, error).
/// </summary>
public static class MessageType
{
    public const string Connect = "connect";
    public const string Chat = "chat";
    public const string Typing = "typing";
    public const string Error = "error";

    public static readonly IReadOnlySet<string> ValidTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Connect,
        Chat,
        Typing,
        Error
    };

    public static bool IsValid(string? type) => !string.IsNullOrWhiteSpace(type) && ValidTypes.Contains(type);
}
