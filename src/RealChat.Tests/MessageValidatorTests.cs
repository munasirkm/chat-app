using RealChat.Api.Models;
using RealChat.Api.Services;

namespace RealChat.Tests;

/// <summary>
/// Unit tests for message validation. Ensures invalid messages are rejected safely (no server crash).
/// </summary>
public class MessageValidatorTests
{
    [Fact]
    public void Validate_NullMessage_ReturnsError()
    {
        var result = MessageValidator.Validate(null!);
        Assert.NotNull(result);
        Assert.Contains("null", result, StringComparison.OrdinalIgnoreCase);
        Assert.False(MessageValidator.IsValid(null));
    }

    [Fact]
    public void Validate_MissingType_ReturnsError()
    {
        var dto = new ChatMessageDto { SenderId = "1", Type = "" };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("type", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_InvalidType_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = "invalid", SenderId = "1" };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("type", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_MissingSenderId_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = MessageType.Chat, SenderId = null, ReceiverId = "2", Data = "Hi" };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("senderId", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_EmptySenderId_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = MessageType.Connect, SenderId = "   " };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_ChatMissingReceiverId_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = MessageType.Chat, SenderId = "1", ReceiverId = null, Data = "Hi" };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("receiverId", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ChatMissingData_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = MessageType.Chat, SenderId = "1", ReceiverId = "2", Data = null! };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("data", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ChatDataExceedsMaxLength_ReturnsError()
    {
        var dto = new ChatMessageDto
        {
            Type = MessageType.Chat,
            SenderId = "1",
            ReceiverId = "2",
            Data = new string('x', MessageValidator.MaxDataLength + 1)
        };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("length", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TypingMissingReceiverId_ReturnsError()
    {
        var dto = new ChatMessageDto { Type = MessageType.Typing, SenderId = "1", ReceiverId = null };
        var result = MessageValidator.Validate(dto);
        Assert.NotNull(result);
        Assert.Contains("receiverId", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ValidConnect_ReturnsNull()
    {
        var dto = new ChatMessageDto { Type = MessageType.Connect, SenderId = "Alice" };
        var result = MessageValidator.Validate(dto);
        Assert.Null(result);
        Assert.True(MessageValidator.IsValid(dto));
    }

    [Fact]
    public void Validate_ValidChat_ReturnsNull()
    {
        var dto = new ChatMessageDto { Type = MessageType.Chat, SenderId = "1", ReceiverId = "2", Data = "Hello" };
        var result = MessageValidator.Validate(dto);
        Assert.Null(result);
        Assert.True(MessageValidator.IsValid(dto));
    }

    [Fact]
    public void Validate_ValidTyping_ReturnsNull()
    {
        var dto = new ChatMessageDto { Type = MessageType.Typing, SenderId = "1", ReceiverId = "2" };
        var result = MessageValidator.Validate(dto);
        Assert.Null(result);
        Assert.True(MessageValidator.IsValid(dto));
    }

    [Fact]
    public void Validate_ChatDataAtMaxLength_ReturnsNull()
    {
        var dto = new ChatMessageDto
        {
            Type = MessageType.Chat,
            SenderId = "1",
            ReceiverId = "2",
            Data = new string('a', MessageValidator.MaxDataLength)
        };
        var result = MessageValidator.Validate(dto);
        Assert.Null(result);
    }

    [Fact]
    public void Validate_TypeCaseInsensitive_AcceptsUpperCase()
    {
        var dto = new ChatMessageDto { Type = "CHAT", SenderId = "1", ReceiverId = "2", Data = "Hi" };
        var result = MessageValidator.Validate(dto);
        Assert.Null(result);
    }
}
