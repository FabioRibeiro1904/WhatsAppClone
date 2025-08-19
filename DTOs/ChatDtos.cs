using WhatsAppClone.Models;

namespace WhatsAppClone.DTOs;

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
    public int ChatId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public int? ReplyToMessageId { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public MessageStatus Status { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public int ChatId { get; set; }
    public int? ReplyToMessageId { get; set; }
    public string? ReplyToContent { get; set; }
    public string? ReplyToSenderName { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsEdited { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
}

public class ChatDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public string? Description { get; set; }
    public string? GroupPicture { get; set; }
    public DateTime LastActivity { get; set; }
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public List<UserDto> Participants { get; set; } = new();
}

public class CreateChatDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; } = false;
    public string? Description { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}

public class TypingIndicatorDto
{
    public int ChatId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
}
