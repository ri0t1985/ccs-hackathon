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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Registration entity
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserDisplayName).IsRequired();
            entity.Property(e => e.FoodRequirements).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure GameRegistration entity
        modelBuilder.Entity<GameRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameId).IsRequired();

            // Configure one-to-many relationship: Registration -> GameRegistration
            entity.HasOne(e => e.Registration)
                .WithMany(r => r.GameRegistrations)
                .HasForeignKey(e => e.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

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
            entity.Property(e => e.GameName).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired();
            entity.Property(e => e.HasAiData).IsRequired();
            entity.Property(e => e.Complexity).HasPrecision(5, 2);
            entity.Property(e => e.TimeToSetupMinutes).IsRequired(false);

            // Configure unique constraint on GameName
            entity.HasIndex(e => e.GameName).IsUnique();
        });
    }
}


