using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealChat.Api.Data;
using RealChat.Api.Models;
using RealChat.Api.Services;

namespace RealChat.Api.Hubs;

public class ChatHub : Hub
{
    private readonly IConnectionTracker _tracker;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatHub(IConnectionTracker tracker, IServiceScopeFactory scopeFactory)
    {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Client joins as a user (by display name). Creates user if needed, registers connection, returns userId.
    /// </summary>
    public async Task<JoinResult> Join(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            await SendErrorToCaller("User name is required.");
            return new JoinResult { Success = false, UserId = 0 };
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

        var user = await GetOrCreateUserAsync(db, userName.Trim());
        var connectionId = Context.ConnectionId!;

        _tracker.Add(connectionId, user.Id);
        await Groups.AddToGroupAsync(connectionId, GroupName(user.Id));

        return new JoinResult { Success = true, UserId = user.Id };
    }

    /// <summary>
    /// Receives a chat message: validates, persists, forwards to receiver. Invalid messages get an error response only.
    /// </summary>
    public async Task SendMessage(ChatMessageDto dto)
    {
        dto.Type = MessageType.Chat;
        var error = MessageValidator.Validate(dto);
        if (error != null)
        {
            await SendErrorToCaller(error);
            return;
        }

        var connectionId = Context.ConnectionId!;
        var senderId = _tracker.GetUserId(connectionId);
        if (senderId == null)
        {
            await SendErrorToCaller("Not registered. Call Join first.");
            return;
        }

        if (!int.TryParse(dto.ReceiverId, out var receiverId))
        {
            await SendErrorToCaller("Invalid receiverId.");
            return;
        }

        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
            var message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Data = dto.Data ?? string.Empty,
                SentAt = DateTime.UtcNow
            };
            db.Messages.Add(message);
            await db.SaveChangesAsync();
        }

        dto.SenderId = senderId.Value.ToString();
        await Clients.Group(GroupName(receiverId)).SendAsync("ReceiveMessage", dto);
    }

    /// <summary>
    /// Typing indicator: validates and forwards to receiver.
    /// </summary>
    public async Task SetTyping(ChatMessageDto dto)
    {
        dto.Type = MessageType.Typing;
        var error = MessageValidator.Validate(dto);
        if (error != null)
        {
            await SendErrorToCaller(error);
            return;
        }

        var connectionId = Context.ConnectionId!;
        var senderId = _tracker.GetUserId(connectionId);
        if (senderId == null)
        {
            await SendErrorToCaller("Not registered. Call Join first.");
            return;
        }

        if (!int.TryParse(dto.ReceiverId, out var receiverId))
        {
            await SendErrorToCaller("Invalid receiverId.");
            return;
        }

        dto.SenderId = senderId.Value.ToString();
        await Clients.Group(GroupName(receiverId)).SendAsync("ReceiveMessage", dto);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _tracker.Remove(Context.ConnectionId!);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendErrorToCaller(string message)
    {
        var dto = new ChatMessageDto
        {
            Type = MessageType.Error,
            Data = message
        };
        await Clients.Caller.SendAsync("ReceiveMessage", dto);
    }

    private static string GroupName(int userId) => "user_" + userId;

    private static async Task<User> GetOrCreateUserAsync(ChatDbContext db, string name)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Name == name);
        if (existing != null) return existing;
        var newUser = new User { Name = name };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }
}

/// <summary>
/// Returned from Join so the client knows its userId.
/// </summary>
public class JoinResult
{
    public bool Success { get; set; }
    public int UserId { get; set; }
}
