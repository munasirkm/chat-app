namespace RealChat.Api.Services;

/// <summary>
/// Tracks SignalR connection id to user id for presence and routing.
/// </summary>
public interface IConnectionTracker
{
    void Add(string connectionId, int userId);
    void Remove(string connectionId);
    int? GetUserId(string connectionId);
    IReadOnlyList<string> GetConnectionIdsForUser(int userId);
    IReadOnlyList<int> GetAllOnlineUserIds();
}
