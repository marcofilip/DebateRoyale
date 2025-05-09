using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI; // Importa la nuova libreria
using System.Threading.Tasks;

namespace DebateRoyale.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;
        private readonly IGenerativeAI _googleAI; // Interfaccia della libreria
        private readonly GenerativeModel _model; // Modello specifico

        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _apiKey = configuration["GeminiApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured in appsettings.json.");
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "LA_TUA_API_KEY_QUI")
            {
                _logger.LogWarning("Gemini API Key is not properly configured. AI analysis will be disabled.");
                // Potresti voler gestire questo caso in modo diverso, ad esempio lanciando un'eccezione
                // o impostando _googleAI e _model a null e controllando prima di usarli.
                // Per ora, l'uso successivo fallirà se l'API key non è valida.
            }

            // Inizializza il client GoogleAI con l'API Key
            _googleAI = new GoogleAI(apiKey: _apiKey);

            // Specifica il modello che vuoi usare (es. Gemini 1.5 Pro, Gemini 1.0 Pro, ecc.)
            // Controlla la classe Model per i nomi corretti o usa stringhe dirette
            // es. "gemini-1.5-pro-latest", "gemini-1.0-pro"
            _model = (GenerativeModel)_googleAI.GenerativeModel(model: Mscc.GenerativeAI.Model.Gemini20Flash); // O un altro modello come Gemini10Pro
            // Se vuoi usare un system prompt di default per tutte le chiamate:
            // var systemInstruction = new Content("You are an impartial debate judge.");
            // _model = (GenerativeModel)_googleAI.GenerativeModel(model: Mscc.GenerativeAI.Model.Gemini15Pro, systemInstruction: systemInstruction);
        }

        public async Task<string> GenerateContentAsync(string promptText)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "LA_TUA_API_KEY_QUI" || _model == null)
            {
                _logger.LogWarning("Gemini Service is not configured correctly (API Key or Model). Returning default message.");
                return "AI analysis is currently unavailable due to configuration issues.";
            }

            try
            {
                _logger.LogInformation("Sending prompt to Gemini: {Prompt}", promptText);

                // Genera il contenuto
                // Puoi anche passare un oggetto GenerateContentRequest più complesso se necessario
                var response = await _model.GenerateContent(promptText);

                if (response != null && !string.IsNullOrEmpty(response.Text))
                {
                    _logger.LogInformation("Received response from Gemini.");
                    return response.Text;
                }
                else if (response != null && response.Candidates != null && response.Candidates.Any())
                {
                    // A volte il testo potrebbe essere in una parte di un candidato
                    // Questa è una gestione più granulare nel caso response.Text sia vuoto
                    var firstCandidate = response.Candidates.First();
                    if (firstCandidate.Content?.Parts != null && firstCandidate.Content.Parts.Any())
                    {
                        var textPart = firstCandidate.Content.Parts.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text));
                        if (textPart != null)
                        {
                            _logger.LogInformation("Received response from Gemini (from candidate part).");
                            return textPart.Text;
                        }
                    }
                }

                _logger.LogWarning("Gemini response was empty or did not contain text. Full response: {@Response}", response);
                return "AI response was empty or did not contain parsable text.";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API via Mscc.GenerativeAI library.");
                // La libreria Mscc.GenerativeAI potrebbe lanciare eccezioni specifiche
                // che potresti voler catturare e gestire diversamente.
                return $"Error calling Gemini API: {ex.Message}";
            }
        }
    }
}