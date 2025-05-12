using System.Collections.Concurrent;
using DebateRoyale.Models;
using Microsoft.AspNetCore.SignalR;
using DebateRoyale.Hubs;

namespace DebateRoyale.Services;

public class ActiveDebateState
{
    public string DebateId { get; } = Guid.NewGuid().ToString();
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
    public ConcurrentDictionary<string, string> Votes { get; } = new ConcurrentDictionary<string, string>();
    public bool IsActive { get; set; } = false;
    public bool IsEnding { get; set; } = false;
}

public class RoomStateService
{
    private readonly ConcurrentDictionary<int, ActiveDebateState> _activeDebates = new();
    private readonly ConcurrentDictionary<string, int> _userConnectionsToRoom = new();
    private readonly IHubContext<StanzaHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GeminiService _geminiService;

    private static readonly Dictionary<int, List<string>> RoomSpecificDebateTopics = new()
    {
        // Stanza ID 1: Cultura Pop
        { 1, new List<string> {
            "I remake dei film classici sono un'opportunità creativa o uno sfruttamento commerciale?",
            "La qualità musicale è diminuita nell'era dello streaming rispetto all'era dei CD?",
            "Gli influencer dei social media rappresentano modelli positivi o dannosi per gli adolescenti?",
            "Le serie TV contemporanee offrono una qualità narrativa superiore ai film?",
            "In che modo il comportamento dei fan accaniti influisce sull'evoluzione delle opere creative?",
            "I fumetti e i graphic novel meritano lo stesso riconoscimento artistico della letteratura classica?",
            "La cultura pop contemporanea sfida o rafforza gli stereotipi sociali?",
            "I programmi di reality televisivi offrono valore culturale o sono mero intrattenimento?",
            "La visione continua di episodi di serie TV (binge-watching) compromette l'apprezzamento delle storie?",
            "Il marketing della nostalgia limita l'innovazione nella cultura pop?"
        }},
        // Stanza ID 2: Innovazione Tecnologica
        { 2, new List<string> {
            "L'intelligenza artificiale creerà più opportunità di lavoro di quante ne eliminerà?",
            "Le piattaforme social dovrebbero essere responsabili dei contenuti pubblicati dagli utenti?",
            "I benefici della tecnologia 5G superano i potenziali rischi?",
            "Quanto influiscono i videogiochi sul comportamento e sullo sviluppo cognitivo?",
            "È possibile bilanciare comodità digitale e protezione della privacy personale?",
            "I veicoli a guida autonoma dovrebbero avere una programmazione etica predefinita?",
            "Un sistema educativo completamente digitale migliorerebbe o peggiorerebbe l'apprendimento?",
            "Come dovrebbero essere tassati i profitti generati dall'automazione?",
            "La crittografia forte dovrebbe includere backdoor per le forze dell'ordine?",
            "Quali limiti dovrebbero essere imposti all'utilizzo di droni nello spazio pubblico e privato?"
        }},
        // Stanza ID 3: Filosofia e Pensiero Critico
        { 3, new List<string> {
            "Il determinismo è compatibile con la responsabilità morale?",
            "La ricerca della felicità è il fine ultimo dell'esistenza umana?",
            "Esistono verità oggettive o tutto è interpretazione soggettiva?",
            "La civiltà è una sottile maschera sulla natura fondamentale dell'essere umano?",
            "I principi etici sono universali o dipendono dal contesto culturale?",
            "L'avanzamento tecnologico ci avvicina o allontana dalla nostra umanità?",
            "Il dolore e la sofferenza sono necessari per la crescita personale?",
            "Una società può progredire senza espressione artistica?",
            "Esistono limiti intrinseci alla conoscenza scientifica?",
            "È moralmente accettabile compiere azioni negative per un bene maggiore?"
        }},
        // Stanza ID 4: Scienza
        { 4, new List<string> {
            "La clonazione umana a scopo terapeutico dovrebbe essere consentita?",
            "Quali alternative alla sperimentazione animale sono scientificamente valide?",
            "La colonizzazione di altri pianeti dovrebbe essere una priorità rispetto alla risoluzione dei problemi terrestri?",
            "L'editing genetico umano rappresenta un'evoluzione naturale della medicina?",
            "L'energia nucleare è componente necessaria nella transizione verso fonti rinnovabili?",
            "Quali interventi sul cambiamento climatico sono più urgenti ed efficaci?",
            "Chi dovrebbe stabilire i limiti etici della ricerca scientifica?",
            "Gli alimenti geneticamente modificati possono risolvere la crisi alimentare globale?",
            "Il metodo scientifico è applicabile a questioni metafisiche?",
            "I benefici della ricerca astronomica giustificano gli investimenti pubblici richiesti?"
        }},
        // Stanza ID 5: Attualità e Politica
        { 5, new List<string> {
            "La globalizzazione economica favorisce lo sviluppo equo o aumenta le disuguaglianze?",
            "Fino a che punto dovrebbe estendersi la regolamentazione governativa delle tecnologie digitali?",
            "L'abbassamento dell'età di voto contribuirebbe a una democrazia più rappresentativa?",
            "La disinformazione online dovrebbe essere combattuta con censura o educazione?",
            "I dibattiti pubblici tra candidati politici dovrebbero essere obbligatori?",
            "Un sistema giudiziario moderno può giustificare l'esistenza della pena capitale?",
            "Le manifestazioni di piazza mantengono rilevanza nell'era dell'attivismo digitale?",
            "Come bilanciare sorveglianza per la sicurezza nazionale e libertà civili?",
            "Quali politiche migrative garantiscono benefici sia al paese ospitante che ai migranti?",
            "I sistemi di voto elettronico possono essere resi sufficientemente sicuri e trasparenti?"
        }},
        // Stanza ID 6: Domande Aperte
        { 6, new List<string> {
            "Le preferenze alimentari controverse (come l'ananas sulla pizza) riflettono differenze culturali?",
            "Gli animali domestici dovrebbero essere scelti in base al carattere o allo stile di vita del proprietario?",
            "La vita urbana offre più opportunità o più stress rispetto a quella rurale?",
            "I compiti a casa rafforzano l'apprendimento o creano disuguaglianze educative?",
            "Come evolverà il concetto di libro nell'era digitale?",
            "Quali riforme del calendario scolastico massimizzerebbero l'efficacia educativa?",
            "I metodi di valutazione tradizionali riflettono adeguatamente le competenze moderne?",
            "Le scelte alimentari personali hanno implicazioni etiche e ambientali?",
            "La prosperità materiale contribuisce al benessere psicologico?",
            "I viaggi solitari e di gruppo offrono esperienze diverse ma ugualmente valide?"
        }}
    };

    private static readonly List<string> FallbackDebateTopics = new()
    {
        "La comunicazione digitale ha migliorato o danneggiato le relazioni interpersonali?"
    };

    private readonly Random _random = new();
    private const int DebateDurationSecondsConfig = 3 * 60;

    public RoomStateService(IHubContext<StanzaHub> hubContext, IServiceScopeFactory scopeFactory, GeminiService geminiService)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _geminiService = geminiService;
    }

    public string GetRandomTopic(int roomId)
    {
        if (RoomSpecificDebateTopics.TryGetValue(roomId, out var specificTopics) && specificTopics.Any())
        {
            return specificTopics[_random.Next(specificTopics.Count)];
        }
        else
        {
            if (FallbackDebateTopics.Any())
            {
                return FallbackDebateTopics[_random.Next(FallbackDebateTopics.Count)];
            }
            else
            {
                return "Discuss any interesting topic!";
            }
        }
    }

    public ActiveDebateState? GetActiveDebate(int roomId) => _activeDebates.TryGetValue(roomId, out var debate) ? debate : null;

    public async Task UserJoinedRoom(int roomId, string connectionId, string userId, string username)
    {
        _userConnectionsToRoom[connectionId] = roomId;
        var debate = _activeDebates.GetOrAdd(roomId, _ => new ActiveDebateState { RoomId = roomId });

        if (debate.IsActive) 
        {
            double remainingSecondsDouble = 0;
            if (debate.StartTime != DateTime.MinValue) 
            {
                TimeSpan totalDebateDuration = TimeSpan.FromSeconds(DebateDurationSecondsConfig);
                TimeSpan elapsed = DateTime.UtcNow - debate.StartTime;
                remainingSecondsDouble = (totalDebateDuration - elapsed).TotalSeconds;

                if (remainingSecondsDouble < 0) remainingSecondsDouble = 0;
            }
            int timeRemainingForSpectator = (int)remainingSecondsDouble;

            bool spectatorHasAlreadyVoted = debate.Votes.ContainsKey(userId);

            int currentVotes1 = debate.Votes.Count(v => v.Value == debate.Debater1UserId);
            int currentVotes2 = debate.Votes.Count(v => v.Value == debate.Debater2UserId);

            await _hubContext.Clients.Client(connectionId).SendAsync("DebateAlreadyInProgress",
                debate.SpecificTopic,
                debate.Debater1Username,
                debate.Debater2Username,
                debate.Transcript.ToList(),
                timeRemainingForSpectator,
                debate.Debater1UserId,
                debate.Debater2UserId,
                spectatorHasAlreadyVoted,
                currentVotes1,
                currentVotes2
            );
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
            await StartDebate(roomId);
        }
        else
        {
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
                    debate.Debater1ConnectionId = null; 
                    if (debate.IsActive && !debate.IsEnding) await EndDebate(roomId, debate.Debater2UserId, $"{leftUsername} left the debate.");
                    wasDebater = true;
                }
                else if (debate.Debater2ConnectionId == connectionId)
                {
                    leftUsername = debate.Debater2Username;
                    debate.Debater2ConnectionId = null; 
                    if (debate.IsActive && !debate.IsEnding) await EndDebate(roomId, debate.Debater1UserId, $"{leftUsername} left the debate.");
                    wasDebater = true;
                }

                if (wasDebater && !debate.IsActive && !string.IsNullOrEmpty(leftUsername)) 
                {
                    await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ParticipantLeft", leftUsername);
                }

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
        debate.SpecificTopic = GetRandomTopic(roomId);
        debate.StartTime = DateTime.UtcNow;
        debate.Transcript.Clear();
        debate.Votes.Clear();
        debate.IsEnding = false;

        const int debateDurationSeconds = DebateDurationSecondsConfig;

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
            return;
        }

        if (debate.Votes.ContainsKey(voterUserId))
        {
            return;
        }

        if (debate.Votes.TryAdd(voterUserId, votedForDebaterUserId))
        {
            int votes1 = debate.Votes.Count(v => v.Value == debate.Debater1UserId);
            int votes2 = debate.Votes.Count(v => v.Value == debate.Debater2UserId);
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("UpdateVotes", votes1, votes2);
        }
        else
        {
        }
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
                string prompt = $"Analizza la seguente trascrizione del dibattito. L'argomento era: \"{debate.SpecificTopic}\". " +
                                $"Il Dibattente1 è {debate.Debater1Username}. Il Dibattente2 è {debate.Debater2Username}. " +
                                $"Determina quale dibattente ha presentato argomentazioni più forti ed è stato più persuasivo. " +
                                $"Fornisci una breve analisi (2-3 frasi) e poi indica chiaramente il vincitore tramite username (ad esempio, 'Vincitore: {debate.Debater1Username}' oppure 'Vincitore: {debate.Debater2Username}'). " +
                                $"Se è un pareggio evidente, scrivi 'Vincitore: Pareggio'.\n\nTrascrizione:\n{transcriptText}";

                aiAnalysis = await _geminiService.GenerateContentAsync(prompt);

                if (aiAnalysis.Contains($"Vincitore: {debate.Debater1Username}")) winnerByAi = debate.Debater1UserId;
                else if (aiAnalysis.Contains($"Vincitore: {debate.Debater2Username}")) winnerByAi = debate.Debater2UserId;
                else if (aiAnalysis.Contains("Vincitore: Pareggio")) winnerByAi = null;
            }
            catch (Exception ex)
            {
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
        if (explicitWinnerId != null)
        {
            finalWinnerId = explicitWinnerId;
            aiAnalysis = reason ?? "Debate ended prematurely.";
        }
        else
        {
            int debater1Score = 0;
            int debater2Score = 0;

            if (debater1SpectatorVotes > 0 || debater2SpectatorVotes > 0)
            {
                if (debater1SpectatorVotes > debater2SpectatorVotes) debater1Score++;
                else if (debater2SpectatorVotes > debater1SpectatorVotes) debater2Score++;
            }
            if (winnerByAi == debate.Debater1UserId) debater1Score++;
            else if (winnerByAi == debate.Debater2UserId) debater2Score++;

            if (debater1Score > debater2Score) finalWinnerId = debate.Debater1UserId;
            else if (debater2Score > debater1Score) finalWinnerId = debate.Debater2UserId;
            else finalWinnerId = null; 
        }

        string winnerUsername = "Tie";
        if (finalWinnerId == debate.Debater1UserId) winnerUsername = debate.Debater1Username ?? "Debater 1";
        else if (finalWinnerId == debate.Debater2UserId) winnerUsername = debate.Debater2Username ?? "Debater 2";

        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("DebateEnded",
            winnerUsername,
            aiAnalysis,
            debater1SpectatorVotes,
            debater2SpectatorVotes);

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
            { 
                if (debate.Debater1UserId != null)
                {
                    var d1User = await dbContext.Users.FindAsync(debate.Debater1UserId);
                }
                if (debate.Debater2UserId != null)
                {
                    var d2User = await dbContext.Users.FindAsync(debate.Debater2UserId);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        _activeDebates.TryRemove(roomId, out _);
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