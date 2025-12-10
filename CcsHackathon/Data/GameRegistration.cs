namespace CcsHackathon.Data;

public class GameRegistration
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string GameId { get; set; } = string.Empty;
    
    // Navigation property for many-to-one relationship with Registration
    public Registration Registration { get; set; } = null!;
    
    // Navigation property for one-to-one relationship with BoardGameCache
    public BoardGameCache? BoardGameCache { get; set; }
}

