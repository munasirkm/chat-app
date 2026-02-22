using RealChat.Api.Services;

namespace RealChat.Api.Api;

/// <summary>
/// Thin API layer: delegates to IChatService, returns HTTP results.
/// </summary>
public static class ChatEndpoints
{
    public static void MapChatApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapGet("/users", GetAllUsers);
        group.MapGet("/users/{userId:int}/conversations", GetRecentConversations);
        group.MapGet("/conversations/{userId:int}/with/{otherUserId:int}/messages", GetMessageHistory);
    }

    private static async Task<IResult> GetAllUsers(IChatService chatService, CancellationToken ct)
    {
        var users = await chatService.GetUsersAsync(ct);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetRecentConversations(int userId, IChatService chatService, CancellationToken ct)
    {
        var conversations = await chatService.GetRecentConversationsAsync(userId, ct);
        return Results.Ok(conversations);
    }

    private static async Task<IResult> GetMessageHistory(int userId, int otherUserId, IChatService chatService, CancellationToken ct)
    {
        var messages = await chatService.GetMessageHistoryAsync(userId, otherUserId, ct);
        return Results.Ok(messages);
    }
}
