using RealChat.Api.Models;

namespace RealChat.Api.Services;

/// <summary>
/// Business logic for chat data: users, conversations, messages, persistence.
/// </summary>
public interface IChatService
{
    Task<User> GetOrCreateUserAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ConversationSummaryDto>> GetRecentConversationsAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<MessageHistoryDto>> GetMessageHistoryAsync(int userId, int otherUserId, CancellationToken ct = default);
    Task SaveMessageAsync(int senderId, int receiverId, string data, CancellationToken ct = default);
}
