using RealChat.Api.Models;

namespace RealChat.Api.Services;

/// <summary>
/// Business logic for chat queries (users, conversations, message history).
/// </summary>
public interface IChatService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ConversationSummaryDto>> GetRecentConversationsAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<MessageHistoryDto>> GetMessageHistoryAsync(int userId, int otherUserId, CancellationToken ct = default);
}
