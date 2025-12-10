namespace CcsHackathon.Data;

public class Registration
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string? FoodRequirements { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? SessionId { get; set; } // Optional - for linking to a session
    
    // Navigation property for many-to-one relationship with Session
    public Session? Session { get; set; }
    
    // Navigation property for one-to-many relationship
    public ICollection<GameRegistration> GameRegistrations { get; set; } = new List<GameRegistration>();
}
