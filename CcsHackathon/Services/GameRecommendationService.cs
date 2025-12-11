using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class GameRecommendationService : IGameRecommendationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGameRatingService _ratingService;

    public GameRecommendationService(ApplicationDbContext dbContext, IGameRatingService ratingService)
    {
        _dbContext = dbContext;
        _ratingService = ratingService;
    }

    public async Task<GameRecommendationResult> GetRecommendationsAsync(string userId)
    {
        // Get all board games with metadata
        var allGames = await _dbContext.BoardGames
            .Include(bg => bg.Metadata)
            .ToListAsync();

        if (!allGames.Any())
        {
            return new GameRecommendationResult
            {
                IsFallbackMode = false,
                Games = new List<RecommendedGame>()
            };
        }

        // Get all user's ratings
        var userRatings = await _dbContext.GameRatings
            .Where(r => r.UserId == userId)
            .GroupBy(r => r.BoardGameId)
            .Select(g => new { BoardGameId = g.Key, AverageRating = (decimal)g.Average(r => r.Rating) })
            .ToDictionaryAsync(x => x.BoardGameId, x => x.AverageRating);

        // Get average ratings for all games
        var allGameIds = allGames.Select(g => g.Id).ToList();
        var averageRatings = await _ratingService.GetAverageRatingsAsync(allGameIds);

        // Get rating counts
        var ratingCounts = await _dbContext.GameRatings
            .Where(r => allGameIds.Contains(r.BoardGameId))
            .GroupBy(r => r.BoardGameId)
            .Select(g => new { BoardGameId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BoardGameId, x => x.Count);

        // Separate rated and unrated games
        var ratedGameIds = userRatings.Keys.ToHashSet();
        var unratedGames = allGames.Where(g => !ratedGameIds.Contains(g.Id)).ToList();
        var ratedGames = allGames.Where(g => ratedGameIds.Contains(g.Id)).ToList();

        // If user has rated all games, return top 3 highest-rated games (fallback mode)
        if (!unratedGames.Any())
        {
            var topRatedGames = ratedGames
                .Select(g => new
                {
                    Game = g,
                    UserRating = userRatings[g.Id],
                    AverageRating = averageRatings.GetValueOrDefault(g.Id)
                })
                .OrderByDescending(x => x.UserRating)
                .ThenByDescending(x => x.AverageRating ?? 0m)
                .Take(3)
                .Select(x => new RecommendedGame
                {
                    BoardGameId = x.Game.Id,
                    GameName = x.Game.Name,
                    Description = x.Game.Description,
                    AverageRating = x.AverageRating,
                    RatingCount = ratingCounts.GetValueOrDefault(x.Game.Id, 0),
                    Complexity = x.Game.SetupComplexity,
                    AveragePlaytimeMinutes = x.Game.AveragePlaytimeMinutes,
                    GameType = x.Game.Metadata?.GameType,
                    Theme = x.Game.Metadata?.Theme,
                    ComplexityTier = x.Game.Metadata?.ComplexityTier,
                    UserRating = x.UserRating
                })
                .ToList();

            return new GameRecommendationResult
            {
                IsFallbackMode = true,
                Games = topRatedGames
            };
        }

        // Calculate similarity scores for unrated games
        var recommendations = unratedGames
            .Select(game => new
            {
                Game = game,
                Score = CalculateSimilarityScore(game, ratedGames, userRatings)
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => new RecommendedGame
            {
                BoardGameId = x.Game.Id,
                GameName = x.Game.Name,
                Description = x.Game.Description,
                AverageRating = averageRatings.GetValueOrDefault(x.Game.Id),
                RatingCount = ratingCounts.GetValueOrDefault(x.Game.Id, 0),
                Complexity = x.Game.SetupComplexity,
                AveragePlaytimeMinutes = x.Game.AveragePlaytimeMinutes,
                GameType = x.Game.Metadata?.GameType,
                Theme = x.Game.Metadata?.Theme,
                ComplexityTier = x.Game.Metadata?.ComplexityTier
            })
            .ToList();

        return new GameRecommendationResult
        {
            IsFallbackMode = false,
            Games = recommendations
        };
    }

    private decimal CalculateSimilarityScore(
        BoardGame candidateGame,
        List<BoardGame> ratedGames,
        Dictionary<Guid, decimal> userRatings)
    {
        if (!ratedGames.Any())
        {
            // If user hasn't rated any games, use average rating as score
            return 0;
        }

        decimal totalScore = 0;
        int matchCount = 0;

        foreach (var ratedGame in ratedGames)
        {
            var userRating = userRatings[ratedGame.Id];
            
            // Only consider games the user rated highly (3+ stars)
            if (userRating < 3)
                continue;

            decimal similarity = 0;

            // Metadata-based similarity
            if (candidateGame.Metadata != null && ratedGame.Metadata != null)
            {
                // Game Type match (high weight)
                if (!string.IsNullOrWhiteSpace(candidateGame.Metadata.GameType) &&
                    !string.IsNullOrWhiteSpace(ratedGame.Metadata.GameType) &&
                    candidateGame.Metadata.GameType.Equals(ratedGame.Metadata.GameType, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 3.0m;
                }

                // Theme match (medium weight)
                if (!string.IsNullOrWhiteSpace(candidateGame.Metadata.Theme) &&
                    !string.IsNullOrWhiteSpace(ratedGame.Metadata.Theme) &&
                    candidateGame.Metadata.Theme.Equals(ratedGame.Metadata.Theme, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 2.0m;
                }

                // Complexity Tier match (medium weight)
                if (!string.IsNullOrWhiteSpace(candidateGame.Metadata.ComplexityTier) &&
                    !string.IsNullOrWhiteSpace(ratedGame.Metadata.ComplexityTier) &&
                    candidateGame.Metadata.ComplexityTier.Equals(ratedGame.Metadata.ComplexityTier, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 2.0m;
                }

                // Player Interaction Level match (low weight)
                if (!string.IsNullOrWhiteSpace(candidateGame.Metadata.PlayerInteractionLevel) &&
                    !string.IsNullOrWhiteSpace(ratedGame.Metadata.PlayerInteractionLevel) &&
                    candidateGame.Metadata.PlayerInteractionLevel.Equals(ratedGame.Metadata.PlayerInteractionLevel, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 1.0m;
                }

                // Typical Play Style match (low weight)
                if (!string.IsNullOrWhiteSpace(candidateGame.Metadata.TypicalPlayStyle) &&
                    !string.IsNullOrWhiteSpace(ratedGame.Metadata.TypicalPlayStyle) &&
                    candidateGame.Metadata.TypicalPlayStyle.Equals(ratedGame.Metadata.TypicalPlayStyle, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 1.0m;
                }
            }

            // Complexity similarity (if both have complexity)
            if (candidateGame.SetupComplexity.HasValue && ratedGame.SetupComplexity.HasValue)
            {
                var complexityDiff = Math.Abs(candidateGame.SetupComplexity.Value - ratedGame.SetupComplexity.Value);
                if (complexityDiff <= 1.0m)
                {
                    similarity += 1.5m * (1.0m - complexityDiff);
                }
            }

            // Playtime similarity (if both have playtime)
            if (candidateGame.AveragePlaytimeMinutes.HasValue && ratedGame.AveragePlaytimeMinutes.HasValue)
            {
                var playtimeDiff = Math.Abs(candidateGame.AveragePlaytimeMinutes.Value - ratedGame.AveragePlaytimeMinutes.Value);
                var maxPlaytime = Math.Max(candidateGame.AveragePlaytimeMinutes.Value, ratedGame.AveragePlaytimeMinutes.Value);
                if (maxPlaytime > 0)
                {
                    var playtimeSimilarity = 1.0m - (Math.Min(playtimeDiff, maxPlaytime) / (decimal)maxPlaytime);
                    similarity += 1.0m * playtimeSimilarity;
                }
            }

            // Weight by user's rating (higher rated games have more influence)
            totalScore += similarity * userRating;
            matchCount++;
        }

        // Average the score
        if (matchCount > 0)
        {
            return totalScore / matchCount;
        }

        return 0;
    }
}

