using Microsoft.EntityFrameworkCore;
using RealChat.Api.Data;
using RealChat.Api.Models;

namespace RealChat.Api.Services;

/// <summary>
/// Implements chat query business logic.
/// </summary>
public class ChatService : IChatService
{
    private readonly ChatDbContext _db;

    public ChatService(ChatDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .OrderBy(u => u.Name)
            .Select(u => new UserDto { Id = u.Id, Name = u.Name })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetRecentConversationsAsync(int userId, CancellationToken ct = default)
    {
        const int limit = 100;
        const int previewLength = 80;

        var messages = await _db.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Take(limit * 2)
            .ToListAsync(ct);

        var seen = new HashSet<int>();
        var list = new List<ConversationSummaryDto>();
        foreach (var m in messages)
        {
            if (list.Count >= limit) break;
            var otherId = m.SenderId == userId ? m.ReceiverId : m.SenderId;
            if (!seen.Add(otherId)) continue;
            var other = m.SenderId == userId ? m.Receiver : m.Sender;
            var preview = m.Data.Length > previewLength ? m.Data[..previewLength] + "..." : m.Data;
            list.Add(new ConversationSummaryDto
            {
                OtherUserId = otherId,
                OtherUserName = other.Name,
                LastMessageAt = m.SentAt,
                LastMessagePreview = preview
            });
        }

        return list;
    }

    public async Task<IReadOnlyList<MessageHistoryDto>> GetMessageHistoryAsync(int userId, int otherUserId, CancellationToken ct = default)
    {
        const int limit = 200;

        return await _db.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .Take(limit)
            .Select(m => new MessageHistoryDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Data = m.Data,
                SentAt = m.SentAt
            })
            .ToListAsync(ct);
    }
}
