using System.ComponentModel.DataAnnotations;

namespace WhatsAppClone.Models;

public class Chat
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsGroup { get; set; } = false;
    
    public string? Description { get; set; }
    
    public string? GroupPicture { get; set; }
    
    public int CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
}
