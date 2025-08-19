namespace WhatsAppClone.Models;

public enum ParticipantRole
{
    Member,
    Admin,
    Owner
}

public class ChatParticipant
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public int ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;
    
    public ParticipantRole Role { get; set; } = ParticipantRole.Member;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastReadAt { get; set; }
    
    public bool IsMuted { get; set; } = false;
    
    public bool IsBlocked { get; set; } = false;
}
