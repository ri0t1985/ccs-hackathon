namespace CcsHackathon.Data;

public class Session
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; } // Date only (no time) for the session
    public DateTime CreatedAt { get; set; }
    public bool IsCancelled { get; set; } // For rescheduling/cancellation
    
    // Navigation property for one-to-many relationship with Registrations
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    
    // Navigation property for many-to-many relationship with games through participants
    public ICollection<GameParticipant> GameParticipants { get; set; } = new List<GameParticipant>();
}

