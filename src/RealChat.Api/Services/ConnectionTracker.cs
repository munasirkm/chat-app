using System.Collections.Concurrent;

namespace RealChat.Api.Services;

/// <summary>
/// In-memory connection-to-user tracking for presence and message routing.
/// </summary>
public sealed class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, int> _connectionToUser = new();

    public void Add(string connectionId, int userId) => _connectionToUser[connectionId] = userId;

    public void Remove(string connectionId) => _connectionToUser.TryRemove(connectionId, out _);

    public int? GetUserId(string connectionId) =>
        _connectionToUser.TryGetValue(connectionId, out var id) ? id : null;

    public IReadOnlyList<string> GetConnectionIdsForUser(int userId) =>
        _connectionToUser.Where(kv => kv.Value == userId).Select(kv => kv.Key).ToList();
}
