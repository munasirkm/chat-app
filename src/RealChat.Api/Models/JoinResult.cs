namespace RealChat.Api.Models;

/// <summary>
/// Returned from ChatHub.Join so the client knows its userId.
/// </summary>
public class JoinResult
{
    public bool Success { get; set; }
    public int UserId { get; set; }
}
