using System.ComponentModel.DataAnnotations;

namespace WhatsAppClone.Models;

public enum MessageType
{
    Text,
    Image,
    File,
    Audio,
    Video
}

public enum MessageStatus
{
    Sent,
    Delivered,
    Read
}

public class Message
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public MessageType Type { get; set; } = MessageType.Text;
    
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    
    public string? FileName { get; set; }
    
    public string? FilePath { get; set; }
    
    public long? FileSize { get; set; }
    
    public int SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;
    
    public int ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;
    
    public int? ReplyToMessageId { get; set; }
    public virtual Message? ReplyToMessage { get; set; }
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? ReadAt { get; set; }
    
    public bool IsEdited { get; set; } = false;
    
    public DateTime? EditedAt { get; set; }
    
    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
}
