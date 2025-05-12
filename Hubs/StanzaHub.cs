using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using DebateRoyale.Services;
using Microsoft.AspNetCore.Authorization;

namespace DebateRoyale.Hubs;

[Authorize] 
public class StanzaHub : Hub
{
    private readonly RoomStateService _roomStateService;

    public StanzaHub(RoomStateService roomStateService)
    {
        _roomStateService = roomStateService;
    }

    public async Task JoinRoom(int roomId)
    {
        var userId = Context.UserIdentifier!;
        var username = Context.User?.Identity?.Name ?? "Anonymous";

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        await _roomStateService.UserJoinedRoom(roomId, Context.ConnectionId, userId, username);
        await Clients.Caller.SendAsync("JoinedRoomSuccess", $"You joined Room {roomId}");
    }

    public async Task SendMessage(int roomId, string message)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        var userId = Context.UserIdentifier!;
        var debate = _roomStateService.GetActiveDebate(roomId);

        if (debate != null && debate.IsActive && (debate.Debater1UserId == userId || debate.Debater2UserId == userId))
        {
            await _roomStateService.AddMessage(roomId, username, message);
        }
        else if (debate == null || !debate.IsActive)
        {
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
        await base.OnDisconnectedAsync(exception);
    }
}