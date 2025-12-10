using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;

namespace CcsHackathon.Services;

public class BoardGameFaqService : IBoardGameFaqService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<BoardGameFaqService> _logger;
    private readonly string? _apiKey;
    private readonly OpenAIService? _service;

    public BoardGameFaqService(
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger<BoardGameFaqService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"];
        
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured. FAQ service will not function.");
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

    public List<string> GetCommonQuestions()
    {
        return new List<string>
        {
            "Explain the setup to me",
            "What type of game is this?",
            "How long does it take to play?",
            "How many players is it best for?",
            "What is the complexity level?",
            "What are the main strategies or tips?",
            "Is this game suitable for children?",
            "Can you summarize the rules?",
            "What expansions are available?",
            "How do I win the game?"
        };
    }

    public async Task<FaqResponse> GetAnswerAsync(Guid boardGameId, string gameName, string question, string userId)
    {
        // Check cache first
        var cachedAnswer = await _dbContext.BoardGameFaqCaches
            .FirstOrDefaultAsync(c => c.BoardGameId == boardGameId && c.Question == question);

        if (cachedAnswer != null)
        {
            _logger.LogInformation("Returning cached FAQ answer for game {GameName}, question: {Question}", gameName, question);
            
            // Create or get conversation and add initial messages
            var conversation = await GetOrCreateConversationAsync(boardGameId, userId);
            await AddMessageToConversationAsync(conversation.Id, "user", question);
            await AddMessageToConversationAsync(conversation.Id, "assistant", cachedAnswer.Answer);
            
            return new FaqResponse
            {
                Answer = cachedAnswer.Answer,
                ConversationId = conversation.Id
            };
        }

        // Generate answer using ChatGPT
        if (_service == null)
        {
            return new FaqResponse
            {
                Answer = "AI service is not available. Please configure the OpenAI API key.",
                ConversationId = Guid.Empty
            };
        }

        try
        {
            _logger.LogInformation("Generating FAQ answer for game {GameName}, question: {Question}", gameName, question);

            var systemPrompt = $"You are a helpful board game expert. Answer questions about the board game \"{gameName}\" in a clear, concise, and friendly manner. Keep responses to 2-3 paragraphs maximum.";
            var userPrompt = $"Question about {gameName}: {question}";

            var messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(systemPrompt),
                ChatMessage.FromUser(userPrompt)
            };

            var completionResult = await _service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messages,
                Model = Models.Gpt_3_5_Turbo,
                Temperature = 0.7f,
                MaxTokens = 500
            });

            if (!completionResult.Successful)
            {
                _logger.LogWarning("OpenAI API call failed for game {GameName}, question {Question}: {Error}", 
                    gameName, question, completionResult.Error?.Message);
                return new FaqResponse
                {
                    Answer = "Sorry, I couldn't generate an answer at this time. Please try again later.",
                    ConversationId = Guid.Empty
                };
            }

            var answer = completionResult.Choices.FirstOrDefault()?.Message?.Content?.Trim() ?? 
                        "Sorry, I couldn't generate an answer at this time.";

            // Cache the answer
            var faqCache = new BoardGameFaqCache
            {
                Id = Guid.NewGuid(),
                BoardGameId = boardGameId,
                Question = question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _dbContext.BoardGameFaqCaches.Add(faqCache);

            // Create or get conversation and add messages
            var conversation = await GetOrCreateConversationAsync(boardGameId, userId);
            await AddMessageToConversationAsync(conversation.Id, "user", question);
            await AddMessageToConversationAsync(conversation.Id, "assistant", answer);

            await _dbContext.SaveChangesAsync();

            return new FaqResponse
            {
                Answer = answer,
                ConversationId = conversation.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating FAQ answer for game {GameName}, question {Question}", gameName, question);
            return new FaqResponse
            {
                Answer = "Sorry, an error occurred while generating the answer. Please try again later.",
                ConversationId = Guid.Empty
            };
        }
    }

    public async Task<string> AskFollowUpAsync(Guid boardGameId, string gameName, string question, string userId, Guid conversationId)
    {
        if (_service == null)
        {
            return "AI service is not available. Please configure the OpenAI API key.";
        }

        try
        {
            // Get conversation history
            var conversation = await _dbContext.BoardGameConversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

            if (conversation == null)
            {
                return "Conversation not found. Please start a new question.";
            }

            _logger.LogInformation("Generating follow-up answer for game {GameName}, question: {Question}", gameName, question);

            // Build conversation context from history
            var messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem($"You are a helpful board game expert. Answer questions about the board game \"{gameName}\" in a clear, concise, and friendly manner. Keep responses to 2-3 paragraphs maximum.")
            };

            // Add conversation history
            foreach (var msg in conversation.Messages)
            {
                if (msg.Role == "user")
                {
                    messages.Add(ChatMessage.FromUser(msg.Content));
                }
                else if (msg.Role == "assistant")
                {
                    messages.Add(ChatMessage.FromAssistant(msg.Content));
                }
            }

            // Add the new question
            messages.Add(ChatMessage.FromUser(question));

            var completionResult = await _service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messages,
                Model = Models.Gpt_3_5_Turbo,
                Temperature = 0.7f,
                MaxTokens = 500
            });

            if (!completionResult.Successful)
            {
                _logger.LogWarning("OpenAI API call failed for follow-up question: {Error}", completionResult.Error?.Message);
                return "Sorry, I couldn't generate an answer at this time. Please try again later.";
            }

            var answer = completionResult.Choices.FirstOrDefault()?.Message?.Content?.Trim() ?? 
                        "Sorry, I couldn't generate an answer at this time.";

            // Add messages to conversation
            await AddMessageToConversationAsync(conversationId, "user", question);
            await AddMessageToConversationAsync(conversationId, "assistant", answer);

            conversation.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return answer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating follow-up answer for conversation {ConversationId}", conversationId);
            return "Sorry, an error occurred while generating the answer. Please try again later.";
        }
    }

    public async Task<List<BoardGameConversationMessage>> GetConversationHistoryAsync(Guid conversationId)
    {
        return await _dbContext.BoardGameConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Guid?> GetConversationIdAsync(Guid boardGameId, string userId)
    {
        var conversation = await _dbContext.BoardGameConversations
            .FirstOrDefaultAsync(c => c.BoardGameId == boardGameId && c.UserId == userId);
        
        return conversation?.Id;
    }

    private async Task<BoardGameConversation> GetOrCreateConversationAsync(Guid boardGameId, string userId)
    {
        var conversation = await _dbContext.BoardGameConversations
            .FirstOrDefaultAsync(c => c.BoardGameId == boardGameId && c.UserId == userId);

        if (conversation == null)
        {
            conversation = new BoardGameConversation
            {
                Id = Guid.NewGuid(),
                BoardGameId = boardGameId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _dbContext.BoardGameConversations.Add(conversation);
            await _dbContext.SaveChangesAsync();
        }

        return conversation;
    }

    private async Task AddMessageToConversationAsync(Guid conversationId, string role, string content)
    {
        var message = new BoardGameConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.BoardGameConversationMessages.Add(message);
        await _dbContext.SaveChangesAsync();
    }
}

