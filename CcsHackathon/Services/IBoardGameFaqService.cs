using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameFaqService
{
    Task<FaqResponse> GetAnswerAsync(Guid boardGameId, string gameName, string question, string userId);
    Task<string> AskFollowUpAsync(Guid boardGameId, string gameName, string question, string userId, Guid conversationId);
    Task<List<BoardGameConversationMessage>> GetConversationHistoryAsync(Guid conversationId);
    Task<Guid?> GetConversationIdAsync(Guid boardGameId, string userId);
    List<string> GetCommonQuestions();
}

public class FaqResponse
{
    public string Answer { get; set; } = string.Empty;
    public Guid ConversationId { get; set; }
}

