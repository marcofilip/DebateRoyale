using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using DebateRoyale.Services; // For RoomStateService
using Microsoft.AspNetCore.Authorization;

namespace DebateRoyale.Hubs;

[Authorize] // Only authenticated users can connect
public class StanzaHub : Hub
{
    private readonly RoomStateService _roomStateService;

    public StanzaHub(RoomStateService roomStateService)
    {
        _roomStateService = roomStateService;
    }

    public async Task JoinRoom(int roomId)
    {
        var userId = Context.UserIdentifier!; // Non-null due to [Authorize]
        var username = Context.User?.Identity?.Name ?? "Anonymous";

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        await _roomStateService.UserJoinedRoom(roomId, Context.ConnectionId, userId, username);
        // Send message to caller that they joined
        await Clients.Caller.SendAsync("JoinedRoomSuccess", $"You joined Room {roomId}");
    }

    public async Task SendMessage(int roomId, string message)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        var userId = Context.UserIdentifier!;
        var debate = _roomStateService.GetActiveDebate(roomId);

        if (debate != null && debate.IsActive && (debate.Debater1UserId == userId || debate.Debater2UserId == userId))
        {
            // Only active debaters can send messages during a debate
            await _roomStateService.AddMessage(roomId, username, message);
        }
        else if (debate == null || !debate.IsActive)
        {
            // Allow chat if debate is not active
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", username, message);
        }
    }

    public async Task CastVote(int roomId, string votedForDebaterUserId)
    {
        var voterUserId = Context.UserIdentifier!;
        await _roomStateService.CastVote(roomId, voterUserId, votedForDebaterUserId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _roomStateService.UserLeftRoom(Context.ConnectionId);
        // The UserLeftRoom method will handle removing from SignalR group if necessary
        // For now, it just updates the RoomStateService. SignalR handles group removal implicitly.
        await base.OnDisconnectedAsync(exception);
    }
}