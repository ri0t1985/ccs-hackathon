using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CcsHackathon.Services;

public class BoardGameAiService : IBoardGameAiService
{
    private readonly ILogger<BoardGameAiService> _logger;
    private readonly string? _apiKey;
    private readonly OpenAIService? _service;

    public BoardGameAiService(IConfiguration configuration, ILogger<BoardGameAiService> logger)
    {
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"];
        
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured. AI service will not function.");
            _service = null;
        }
        else
        {
            _service = new OpenAIService(new OpenAiOptions
            {
                ApiKey = _apiKey
            });
        }
    }

    public async Task<BoardGameAiData?> GenerateAiDataAsync(string gameName)
    {
        if (_service == null)
        {
            _logger.LogWarning("OpenAI service not initialized. Skipping AI data generation for {GameName}", gameName);
            return null;
        }

        try
        {
            _logger.LogInformation("Generating AI data for board game: {GameName}", gameName);

            var prompt = $@"Analyze the board game ""{gameName}"" and provide the following information in JSON format:
{{
  ""complexity"": <a decimal number from 1.0 to 5.0 representing game complexity, where 1.0 is very simple and 5.0 is very complex>,
  ""timeToSetupMinutes"": <an integer representing the estimated time in minutes to set up the game>,
  ""summary"": ""<a brief 2-3 sentence summary of the game, its mechanics, and what makes it interesting>""
}}

Only return valid JSON, no additional text or markdown formatting.";

            var messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem("You are a board game expert. Provide accurate, concise information about board games in JSON format."),
                ChatMessage.FromUser(prompt)
            };

            var completionResult = await _service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messages,
                Model = Models.Gpt_3_5_Turbo,
                Temperature = 0.3f,
                MaxTokens = 500
            });

            if (!completionResult.Successful)
            {
                _logger.LogWarning("OpenAI API call failed for game {GameName}: {Error}", gameName, completionResult.Error?.Message);
                return null;
            }

            var content = completionResult.Choices.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Empty response from OpenAI for game: {GameName}", gameName);
                return null;
            }

            // Clean up the response - remove markdown code blocks if present
            content = Regex.Replace(content, @"^```json\s*", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"^```\s*$", "", RegexOptions.Multiline);
            content = content.Trim();

            var aiData = JsonSerializer.Deserialize<BoardGameAiData>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (aiData == null)
            {
                _logger.LogWarning("Failed to parse AI response for game: {GameName}. Response: {Response}", gameName, content);
                return null;
            }

            // Validate the data
            if (aiData.Complexity < 1.0m || aiData.Complexity > 5.0m)
            {
                _logger.LogWarning("Invalid complexity value {Complexity} for game {GameName}. Clamping to valid range.", aiData.Complexity, gameName);
                aiData.Complexity = Math.Clamp(aiData.Complexity, 1.0m, 5.0m);
            }

            if (aiData.TimeToSetupMinutes < 0)
            {
                _logger.LogWarning("Invalid time to setup {Time} for game {GameName}. Setting to 0.", aiData.TimeToSetupMinutes, gameName);
                aiData.TimeToSetupMinutes = 0;
            }

            _logger.LogInformation("Successfully generated AI data for game: {GameName}. Complexity: {Complexity}, Setup Time: {Time}min", 
                gameName, aiData.Complexity, aiData.TimeToSetupMinutes);

            return aiData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI data for game: {GameName}", gameName);
            return null;
        }
    }
}
