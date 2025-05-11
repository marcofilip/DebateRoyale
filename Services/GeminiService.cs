using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using System.Threading.Tasks;

namespace DebateRoyale.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;
        private readonly IGenerativeAI _googleAI;
        private readonly GenerativeModel _model;

        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _apiKey = configuration["GeminiApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured in appsettings.json.");
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "LA_TUA_API_KEY_QUI")
            {
                _logger.LogWarning("Gemini API Key is not properly configured. AI analysis will be disabled.");
            }

            _googleAI = new GoogleAI(apiKey: _apiKey);

            _model = (GenerativeModel)_googleAI.GenerativeModel(model: Mscc.GenerativeAI.Model.Gemini20Flash); 
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

                var response = await _model.GenerateContent(promptText);

                if (response != null && !string.IsNullOrEmpty(response.Text))
                {
                    _logger.LogInformation("Received response from Gemini.");
                    return response.Text;
                }
                else if (response != null && response.Candidates != null && response.Candidates.Any())
                {
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
                return $"Error calling Gemini API: {ex.Message}";
            }
        }
    }
}