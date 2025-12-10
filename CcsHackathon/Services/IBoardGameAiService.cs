using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameAiService
{
    Task<BoardGameAiData?> GenerateAiDataAsync(string gameName);
}

public class BoardGameAiData
{
    public decimal Complexity { get; set; }
    public int TimeToSetupMinutes { get; set; }
    public string Summary { get; set; } = string.Empty;
}

