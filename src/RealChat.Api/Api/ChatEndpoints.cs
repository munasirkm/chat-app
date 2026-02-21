using Microsoft.EntityFrameworkCore;
using RealChat.Api.Data;
using RealChat.Api.Models;

namespace RealChat.Api.Api;

public static class ChatEndpoints
{
    private const int RecentConversationsLimit = 100;
    private const int MessageHistoryLimit = 200;
    private const int PreviewLength = 80;

    public static void MapChatApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapGet("/users", GetAllUsers);
        group.MapGet("/users/{userId:int}/conversations", GetRecentConversations);
        group.MapGet("/conversations/{userId:int}/with/{otherUserId:int}/messages", GetMessageHistory);
    }

    private static async Task<IResult> GetAllUsers(ChatDbContext db)
    {
        var users = await db.Users
            .OrderBy(u => u.Name)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();
        return Results.Ok(users);
    }

    private static async Task<IResult> GetRecentConversations(int userId, ChatDbContext db)
    {
        var messages = await db.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Take(RecentConversationsLimit * 2)
            .ToListAsync();

        var seen = new HashSet<int>();
        var list = new List<ConversationSummaryDto>();
        foreach (var m in messages)
        {
            if (list.Count >= RecentConversationsLimit) break;
            var otherId = m.SenderId == userId ? m.ReceiverId : m.SenderId;
            if (!seen.Add(otherId)) continue;
            var other = m.SenderId == userId ? m.Receiver : m.Sender;
            var preview = m.Data.Length > PreviewLength ? m.Data[..PreviewLength] + "..." : m.Data;
            list.Add(new ConversationSummaryDto
            {
                OtherUserId = otherId,
                OtherUserName = other.Name,
                LastMessageAt = m.SentAt,
                LastMessagePreview = preview
            });
        }

        return Results.Ok(list);
    }

    private static async Task<IResult> GetMessageHistory(int userId, int otherUserId, ChatDbContext db)
    {
        var messages = await db.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .Take(MessageHistoryLimit)
            .Select(m => new MessageHistoryDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Data = m.Data,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return Results.Ok(messages);
    }
}
