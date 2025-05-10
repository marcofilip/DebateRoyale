using System.Collections.Concurrent;
using DebateRoyale.Models;
using Microsoft.AspNetCore.SignalR;
using DebateRoyale.Hubs; // Assicurati che l'hub sia referenziato correttamente

namespace DebateRoyale.Services;

public class ActiveDebateState
{
    public string DebateId { get; } = Guid.NewGuid().ToString(); // Unique ID for this active session
    public int RoomId { get; set; }
    public string SpecificTopic { get; set; } = "Not set";
    public string? Debater1ConnectionId { get; set; }
    public string? Debater1UserId { get; set; }
    public string? Debater1Username { get; set; }
    public string? Debater2ConnectionId { get; set; }
    public string? Debater2UserId { get; set; }
    public string? Debater2Username { get; set; }
    public DateTime StartTime { get; set; }
    public Timer? DebateTimer { get; set; }
    public List<string> Transcript { get; } = new List<string>();
    public ConcurrentDictionary<string, string> Votes { get; } = new ConcurrentDictionary<string, string>(); // VoterUserId, VotedForDebaterUserId
    public bool IsActive { get; set; } = false;
    public bool IsEnding { get; set; } = false;
}

public class RoomStateService
{
    // RoomId -> ActiveDebateState
    private readonly ConcurrentDictionary<int, ActiveDebateState> _activeDebates = new();
    private readonly ConcurrentDictionary<string, int> _userConnectionsToRoom = new(); // ConnectionId -> RoomId
    private readonly IHubContext<StanzaHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GeminiService _geminiService;

    private static readonly List<string> GeneralDebateTopics = new()
    {
        "Is social media more beneficial or harmful to society?",
        "Should remote work be the new standard?",
        "Is artificial intelligence a threat to humanity?",
        "Should pineapple be on pizza?",
        "Cats vs. Dogs: Which make better pets?",
        "Is space exploration worth the investment?",
        "Should voting be mandatory?",
        "Is a universal basic income a viable solution to poverty?"
    };
    private readonly Random _random = new();

    public RoomStateService(IHubContext<StanzaHub> hubContext, IServiceScopeFactory scopeFactory, GeminiService geminiService)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _geminiService = geminiService;
    }

    public string GetRandomTopic() => GeneralDebateTopics[_random.Next(GeneralDebateTopics.Count)];

    public ActiveDebateState? GetActiveDebate(int roomId) => _activeDebates.TryGetValue(roomId, out var debate) ? debate : null;

    public async Task UserJoinedRoom(int roomId, string connectionId, string userId, string username)
    {
        _userConnectionsToRoom[connectionId] = roomId;
        var debate = _activeDebates.GetOrAdd(roomId, _ => new ActiveDebateState { RoomId = roomId });

        if (debate.IsActive)
        {
            // User is joining as a spectator to an active debate
            await _hubContext.Clients.Client(connectionId).SendAsync("DebateAlreadyInProgress", debate.SpecificTopic,
                debate.Debater1Username, debate.Debater2Username, debate.Transcript, (int)(debate.StartTime.AddMinutes(5) - DateTime.UtcNow).TotalSeconds);
        }
        else if (string.IsNullOrEmpty(debate.Debater1ConnectionId) && debate.Debater1UserId != userId)
        {
            debate.Debater1ConnectionId = connectionId;
            debate.Debater1UserId = userId;
            debate.Debater1Username = username;
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ParticipantJoined", username, 1);
        }
        else if (string.IsNullOrEmpty(debate.Debater2ConnectionId) && debate.Debater2UserId != userId && debate.Debater1UserId != userId)
        {
            debate.Debater2ConnectionId = connectionId;
            debate.Debater2UserId = userId;
            debate.Debater2Username = username;
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ParticipantJoined", username, 2);
            // Both debaters are here, start the debate
            await StartDebate(roomId);
        }
        else
        {
            // User is a spectator, or trying to join a full waiting room
            await _hubContext.Clients.Client(connectionId).SendAsync("SpectatorJoined", debate.Debater1Username, debate.Debater2Username);
        }
    }

    public async Task UserLeftRoom(string connectionId)
    {
        if (_userConnectionsToRoom.TryRemove(connectionId, out var roomId))
        {
            var debate = GetActiveDebate(roomId);
            if (debate != null)
            {
                bool wasDebater = false;
                string? leftUsername = null;

                if (debate.Debater1ConnectionId == connectionId)
                {
                    leftUsername = debate.Debater1Username;
                    debate.Debater1ConnectionId = null; // Allow new debater1
                    // If debate was active, the other debater wins by forfeit
                    if (debate.IsActive && !debate.IsEnding) await EndDebate(roomId, debate.Debater2UserId, $"{leftUsername} left the debate.");
                    wasDebater = true;
                }
                else if (debate.Debater2ConnectionId == connectionId)
                {
                    leftUsername = debate.Debater2Username;
                    debate.Debater2ConnectionId = null; // Allow new debater2
                    if (debate.IsActive && !debate.IsEnding) await EndDebate(roomId, debate.Debater1UserId, $"{leftUsername} left the debate.");
                    wasDebater = true;
                }

                if (wasDebater && !debate.IsActive && !string.IsNullOrEmpty(leftUsername)) // if debate was not active yet
                {
                    await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ParticipantLeft", leftUsername);
                }

                // If no debaters left and debate not active, clean up.
                if (string.IsNullOrEmpty(debate.Debater1ConnectionId) && string.IsNullOrEmpty(debate.Debater2ConnectionId) && !debate.IsActive)
                {
                    _activeDebates.TryRemove(roomId, out _);
                }
            }
        }
    }

    private async Task StartDebate(int roomId)
    {
        var debate = GetActiveDebate(roomId);
        if (debate == null || debate.IsActive || string.IsNullOrEmpty(debate.Debater1ConnectionId) || string.IsNullOrEmpty(debate.Debater2ConnectionId))
            return;

        debate.IsActive = true;
        debate.SpecificTopic = GetRandomTopic();
        debate.StartTime = DateTime.UtcNow;
        debate.Transcript.Clear();
        debate.Votes.Clear();
        debate.IsEnding = false;

        const int debateDurationSeconds = 3 * 60;

        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("DebateStarted",
            debate.SpecificTopic, debate.Debater1Username, debate.Debater2Username, debate.Debater1UserId, debate.Debater2UserId, debateDurationSeconds);

        debate.DebateTimer = new Timer(async _ => await EndDebate(roomId), null, TimeSpan.FromSeconds(debateDurationSeconds), Timeout.InfiniteTimeSpan);
    }

    public async Task AddMessage(int roomId, string username, string message)
    {
        var debate = GetActiveDebate(roomId);
        if (debate == null || !debate.IsActive) return;

        var formattedMessage = $"{username}: {message}";
        debate.Transcript.Add(formattedMessage);
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", username, message);
    }

    public async Task CastVote(int roomId, string voterUserId, string votedForDebaterUserId)
    {
        var debate = GetActiveDebate(roomId);
        if (debate == null || !debate.IsActive || debate.Debater1UserId == voterUserId || debate.Debater2UserId == voterUserId)
        {
            // Can't vote for self or if not active debate
            return;
        }
        debate.Votes[voterUserId] = votedForDebaterUserId; // Overwrites previous vote if any

        int votes1 = debate.Votes.Count(v => v.Value == debate.Debater1UserId);
        int votes2 = debate.Votes.Count(v => v.Value == debate.Debater2UserId);
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("UpdateVotes", votes1, votes2);
    }

    public async Task EndDebate(int roomId, string? explicitWinnerId = null, string? reason = null)
    {
        var debate = GetActiveDebate(roomId);
        if (debate == null || !debate.IsActive || debate.IsEnding) return;

        debate.IsEnding = true;
        debate.DebateTimer?.Dispose();
        debate.IsActive = false;

        string transcriptText = string.Join("\n", debate.Transcript);
        string aiAnalysis = "AI analysis disabled or failed.";
        string? winnerByAi = null;

        if (!string.IsNullOrWhiteSpace(transcriptText))
        {
            try
            {
                // Prompt for Gemini
                string prompt = $"Analyze the following debate transcript. The topic was: \"{debate.SpecificTopic}\". " +
                                $"Debater1 is {debate.Debater1Username}. Debater2 is {debate.Debater2Username}. " +
                                $"Determine which debater presented stronger arguments and was more persuasive. " +
                                $"Provide a brief analysis (2-3 sentences) and then state the winner clearly by username (e.g., 'Winner: {debate.Debater1Username}' or 'Winner: {debate.Debater2Username}'). " +
                                $"If it's a clear tie, state 'Winner: Tie'.\n\nTranscript:\n{transcriptText}";

                aiAnalysis = await _geminiService.GenerateContentAsync(prompt);

                // Simple parsing for winner based on AI text
                if (aiAnalysis.Contains($"Winner: {debate.Debater1Username}")) winnerByAi = debate.Debater1UserId;
                else if (aiAnalysis.Contains($"Winner: {debate.Debater2Username}")) winnerByAi = debate.Debater2UserId;
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                aiAnalysis = $"AI analysis failed: {ex.Message}";
            }
        }
        else
        {
            aiAnalysis = "No messages were exchanged in the debate.";
        }


        int debater1SpectatorVotes = debate.Votes.Count(v => v.Value == debate.Debater1UserId);
        int debater2SpectatorVotes = debate.Votes.Count(v => v.Value == debate.Debater2UserId);

        string? finalWinnerId = null;
        if (explicitWinnerId != null) // e.g. someone left
        {
            finalWinnerId = explicitWinnerId;
            aiAnalysis = reason ?? "Debate ended prematurely.";
        }
        else
        {
            // Determine winner: 50% spectator, 50% AI
            // 1 point for winning spectator vote, 1 point for winning AI vote
            int debater1Score = 0;
            int debater2Score = 0;

            if (debater1SpectatorVotes > debater2SpectatorVotes) debater1Score++;
            else if (debater2SpectatorVotes > debater1SpectatorVotes) debater2Score++;

            if (winnerByAi == debate.Debater1UserId) debater1Score++;
            else if (winnerByAi == debate.Debater2UserId) debater2Score++;

            if (debater1Score > debater2Score) finalWinnerId = debate.Debater1UserId;
            else if (debater2Score > debater1Score) finalWinnerId = debate.Debater2UserId;
            else finalWinnerId = null; // Tie
        }

        string winnerUsername = "Tie";
        if (finalWinnerId == debate.Debater1UserId) winnerUsername = debate.Debater1Username ?? "Debater 1";
        else if (finalWinnerId == debate.Debater2UserId) winnerUsername = debate.Debater2Username ?? "Debater 2";

        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("DebateEnded",
            winnerUsername,
            aiAnalysis,
            debater1SpectatorVotes,
            debater2SpectatorVotes);

        // Persist debate to DB
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var newDebate = new Models.Debate
            {
                RoomId = roomId,
                SpecificTopic = debate.SpecificTopic,
                Debater1Id = debate.Debater1UserId,
                Debater2Id = debate.Debater2UserId,
                StartTime = debate.StartTime,
                EndTime = DateTime.UtcNow,
                Transcript = transcriptText,
                AiAnalysis = aiAnalysis,
                WinnerId = finalWinnerId,
                Debater1Votes = debater1SpectatorVotes,
                Debater2Votes = debater2SpectatorVotes,
                IsActive = false
            };
            dbContext.Debates.Add(newDebate);

            if (finalWinnerId != null)
            {
                var winnerUser = await dbContext.Users.FindAsync(finalWinnerId);
                if (winnerUser != null) winnerUser.Wins++;

                var loserId = (finalWinnerId == debate.Debater1UserId) ? debate.Debater2UserId : debate.Debater1UserId;
                if (loserId != null)
                {
                    var loserUser = await dbContext.Users.FindAsync(loserId);
                    if (loserUser != null) loserUser.Losses++;
                }
            }
            else
            { // Tie, or only one participant.
                if (debate.Debater1UserId != null)
                {
                    var d1User = await dbContext.Users.FindAsync(debate.Debater1UserId);
                    // if (d1User != null) d1User.Losses++; // Or some other stat for a tie.
                }
                if (debate.Debater2UserId != null)
                {
                    var d2User = await dbContext.Users.FindAsync(debate.Debater2UserId);
                    // if (d2User != null) d2User.Losses++;
                }
            }
            await dbContext.SaveChangesAsync();
        }

        // Reset state for the room to allow a new debate
        _activeDebates.TryRemove(roomId, out _);
        // Users are still in the SignalR group, they can start a new debate by re-signaling readiness
        // or the UI can prompt them. For simplicity, we fully reset.
        // New participants joining will trigger UserJoinedRoom and potentially a new debate.
    }

    public Dictionary<int, int> GetActiveUserCountsPerRoom()
    {
        var counts = new Dictionary<int, int>();
        foreach (var group in _userConnectionsToRoom.GroupBy(conn => conn.Value))
        {
            counts[group.Key] = group.Count();
        }
        return counts;
    }
}