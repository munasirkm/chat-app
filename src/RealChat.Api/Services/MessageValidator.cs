using RealChat.Api.Models;

namespace RealChat.Api.Services;

/// <summary>
/// Validates incoming chat message DTOs. Invalid messages are rejected safely (no server crash).
/// </summary>
public static class MessageValidator
{
    public const int MaxDataLength = 4_096;

    /// <summary>
    /// Validates the message format. Returns null if valid; otherwise returns an error message.
    /// </summary>
    public static string? Validate(ChatMessageDto? message)
    {
        if (message == null)
            return "Message is null.";

        if (!MessageType.IsValid(message.Type))
            return "Invalid or missing 'type'. Must be one of: connect, chat, typing, error.";

        if (string.IsNullOrWhiteSpace(message.SenderId))
            return "Missing or empty 'senderId'.";

        switch (message.Type.ToLowerInvariant())
        {
            case MessageType.Connect:
                // connect only needs senderId
                return null;

            case MessageType.Chat:
                if (string.IsNullOrWhiteSpace(message.ReceiverId))
                    return "Missing or empty 'receiverId' for chat message.";
                if (message.Data == null)
                    return "Missing 'data' for chat message.";
                if (message.Data.Length > MaxDataLength)
                    return $"Message 'data' exceeds maximum length of {MaxDataLength}.";
                return null;

            case MessageType.Typing:
                if (string.IsNullOrWhiteSpace(message.ReceiverId))
                    return "Missing or empty 'receiverId' for typing indicator.";
                return null;

            case MessageType.Error:
                // error can be sent by server; minimal validation
                return null;

            default:
                return "Unknown message type.";
        }
    }

    /// <summary>
    /// Returns true if the message is valid.
    /// </summary>
    public static bool IsValid(ChatMessageDto? message) => Validate(message) == null;
}
