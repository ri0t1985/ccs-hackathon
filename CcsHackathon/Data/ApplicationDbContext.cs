using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

        public DbSet<Registration> Registrations { get; set; }
        public DbSet<GameRegistration> GameRegistrations { get; set; }
        public DbSet<BoardGameCache> BoardGameCaches { get; set; }
        public DbSet<BoardGame> BoardGames { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<BoardGameFaqCache> BoardGameFaqCaches { get; set; }
        public DbSet<BoardGameConversation> BoardGameConversations { get; set; }
        public DbSet<BoardGameConversationMessage> BoardGameConversationMessages { get; set; }
        public DbSet<GameRating> GameRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date)
                .IsRequired()
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsCancelled).IsRequired();
            
            // Ensure unique constraint on Date (one session per day)
            entity.HasIndex(e => e.Date).IsUnique();
        });

        // Configure Registration entity
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserDisplayName).IsRequired();
            entity.Property(e => e.FoodRequirements).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.SessionId).IsRequired(false);

            // Configure many-to-one relationship: Registration -> Session
            entity.HasOne(e => e.Session)
                .WithMany(s => s.Registrations)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.SetNull); // Don't cascade delete sessions when registration is deleted
        });

        // Configure BoardGame entity
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired(false);
            entity.Property(e => e.SetupComplexity).HasPrecision(5, 2);
            entity.Property(e => e.Score).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired(false);

            // Ensure unique constraint on Name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure GameRegistration entity
        modelBuilder.Entity<GameRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BoardGameId).IsRequired();

            // Configure one-to-many relationship: Registration -> GameRegistration
            entity.HasOne(e => e.Registration)
                .WithMany(r => r.GameRegistrations)
                .HasForeignKey(e => e.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-one relationship: GameRegistration -> BoardGame
            entity.HasOne(e => e.BoardGame)
                .WithMany(bg => bg.GameRegistrations)
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting board games that are referenced

            // Configure one-to-one relationship: GameRegistration -> BoardGameCache
            entity.HasOne(e => e.BoardGameCache)
                .WithOne(b => b.GameRegistration)
                .HasForeignKey<BoardGameCache>(b => b.GameRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure BoardGameCache entity
        modelBuilder.Entity<BoardGameCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameRegistrationId).IsRequired();
            entity.Property(e => e.BoardGameId).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired();
            entity.Property(e => e.HasAiData).IsRequired();
            entity.Property(e => e.Complexity).HasPrecision(5, 2);
            entity.Property(e => e.TimeToSetupMinutes).IsRequired(false);

            // Configure many-to-one relationship: BoardGameCache -> BoardGame
            entity.HasOne(e => e.BoardGame)
                .WithMany(bg => bg.BoardGameCaches)
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting board games that are referenced
        });

        // Configure BoardGameFaqCache entity
        modelBuilder.Entity<BoardGameFaqCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BoardGameId).IsRequired();
            entity.Property(e => e.Question).IsRequired();
            entity.Property(e => e.Answer).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired();
            
            // Configure many-to-one relationship: BoardGameFaqCache -> BoardGame
            entity.HasOne(e => e.BoardGame)
                .WithMany()
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for faster lookups
            entity.HasIndex(e => new { e.BoardGameId, e.Question });
        });

        // Configure BoardGameConversation entity
        modelBuilder.Entity<BoardGameConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BoardGameId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired();
            
            // Configure many-to-one relationship: BoardGameConversation -> BoardGame
            entity.HasOne(e => e.BoardGame)
                .WithMany()
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure one-to-many relationship: BoardGameConversation -> BoardGameConversationMessage
            entity.HasMany(e => e.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for faster lookups
            entity.HasIndex(e => new { e.BoardGameId, e.UserId });
        });

        // Configure BoardGameConversationMessage entity
        modelBuilder.Entity<BoardGameConversationMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversationId).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Configure many-to-one relationship: BoardGameConversationMessage -> BoardGameConversation
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GameRating entity
        modelBuilder.Entity<GameRating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.BoardGameId).IsRequired();
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Validate rating range
            entity.HasCheckConstraint("CK_GameRating_Rating", "[Rating] >= 0 AND [Rating] <= 5");
            
            // Configure many-to-one relationship: GameRating -> BoardGame
            entity.HasOne(e => e.BoardGame)
                .WithMany()
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure many-to-one relationship: GameRating -> Session
            entity.HasOne(e => e.Session)
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Unique constraint: one rating per user per game per session
            entity.HasIndex(e => new { e.UserId, e.BoardGameId, e.SessionId }).IsUnique();
        });
    }
}


