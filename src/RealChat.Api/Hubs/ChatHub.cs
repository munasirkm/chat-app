using Microsoft.AspNetCore.SignalR;
using RealChat.Api.Models;
using RealChat.Api.Services;

namespace RealChat.Api.Hubs;

/// <summary>
/// SignalR hub: orchestrates connection tracking, validation, service calls, and broadcasting.
/// No direct data access; delegates persistence to IChatService.
/// </summary>
public class ChatHub : Hub
{
    private readonly IConnectionTracker _tracker;
    private readonly IChatService _chatService;

    public ChatHub(IConnectionTracker tracker, IChatService chatService)
    {
        _tracker = tracker;
        _chatService = chatService;
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

        var user = await _chatService.GetOrCreateUserAsync(userName.Trim());
        var connectionId = Context.ConnectionId!;

        _tracker.Add(connectionId, user.Id);
        await Groups.AddToGroupAsync(connectionId, HubGroups.User(user.Id));
        await Groups.AddToGroupAsync(connectionId, HubGroups.All);
        await Clients.Group(HubGroups.All).SendAsync("UserOnline", user.Id);

        return new JoinResult { Success = true, UserId = user.Id };
    }

    /// <summary>
    /// Returns the list of user ids currently connected (for presence).
    /// </summary>
    public IReadOnlyList<int> GetOnlineUserIds() => _tracker.GetAllOnlineUserIds();

    /// <summary>
    /// Receives a chat message: validates, persists via service, forwards to receiver.
    /// </summary>
    public async Task SendMessage(ChatMessageDto dto)
    {
        var (senderId, error) = ResolveAndValidateSender();
        if (error != null) { await SendErrorToCaller(error); return; }

        dto.Type = MessageType.Chat;
        dto.SenderId = senderId!.Value.ToString();
        var validationError = MessageValidator.Validate(dto);
        if (validationError != null) { await SendErrorToCaller(validationError); return; }

        if (!int.TryParse(dto.ReceiverId, out var receiverId))
        { await SendErrorToCaller("Invalid receiverId."); return; }

        await _chatService.SaveMessageAsync(senderId.Value, receiverId, dto.Data ?? string.Empty);

        await Clients.Group(HubGroups.User(receiverId)).SendAsync("ReceiveMessage", dto);
    }

    /// <summary>
    /// Typing indicator: validates and forwards to receiver.
    /// </summary>
    public async Task SetTyping(ChatMessageDto dto)
    {
        var (senderId, error) = ResolveAndValidateSender();
        if (error != null) { await SendErrorToCaller(error); return; }

        dto.Type = MessageType.Typing;
        dto.SenderId = senderId!.Value.ToString();
        var validationError = MessageValidator.Validate(dto);
        if (validationError != null) { await SendErrorToCaller(validationError); return; }

        if (!int.TryParse(dto.ReceiverId, out var receiverId))
        { await SendErrorToCaller("Invalid receiverId."); return; }

        await Clients.Group(HubGroups.User(receiverId)).SendAsync("ReceiveMessage", dto);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId!;
        var userId = _tracker.GetUserId(connectionId);
        _tracker.Remove(connectionId);
        if (userId != null)
            await Clients.Group(HubGroups.All).SendAsync("UserOffline", userId.Value);
        await base.OnDisconnectedAsync(exception);
    }

    private (int? SenderId, string? Error) ResolveAndValidateSender()
    {
        var senderId = _tracker.GetUserId(Context.ConnectionId!);
        return senderId == null ? (null, "Not registered. Call Join first.") : (senderId, null);
    }

    private async Task SendErrorToCaller(string message)
    {
        await Clients.Caller.SendAsync("ReceiveMessage", new ChatMessageDto { Type = MessageType.Error, Data = message });
    }
}

/// <summary>
/// SignalR group names for routing.
/// </summary>
internal static class HubGroups
{
    public const string All = "all";
    public static string User(int userId) => "user_" + userId;
}
