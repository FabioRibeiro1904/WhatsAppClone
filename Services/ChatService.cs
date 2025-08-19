using Microsoft.EntityFrameworkCore;
using WhatsAppClone.Data;
using WhatsAppClone.DTOs;
using WhatsAppClone.Models;

namespace WhatsAppClone.Services;

public interface IChatService
{
    Task<List<ChatDto>> GetUserChatsAsync(int userId);
    Task<ChatDto?> GetChatByIdAsync(int chatId, int userId);
    Task<ChatDto?> CreateChatAsync(CreateChatDto createChatDto, int createdById);
    Task<ChatDto?> GetOrCreatePrivateChatAsync(int user1Id, int user2Id);
    Task<List<MessageDto>> GetChatMessagesAsync(int chatId, int userId, int page = 0, int pageSize = 50);
    Task<MessageDto?> SendMessageAsync(SendMessageDto sendMessageDto, int senderId);
    Task<bool> MarkMessagesAsReadAsync(int chatId, int userId);
    Task<bool> AddParticipantAsync(int chatId, int userId, int addedByUserId);
    Task<bool> RemoveParticipantAsync(int chatId, int userId, int removedByUserId);
    Task<bool> UpdateChatAsync(int chatId, string name, string? description, int updatedByUserId);
}

public class ChatService : IChatService
{
    private readonly WhatsAppDbContext _context;

    public ChatService(WhatsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatDto>> GetUserChatsAsync(int userId)
    {
        var chats = await _context.ChatParticipants
            .Where(cp => cp.UserId == userId)
            .Include(cp => cp.Chat)
                .ThenInclude(c => c.Participants)
                .ThenInclude(p => p.User)
            .Select(cp => cp.Chat)
            .OrderByDescending(c => c.LastActivity)
            .ToListAsync();

        var result = new List<ChatDto>();
        
        foreach (var chat in chats)
        {
            var lastMessage = await _context.Messages
                .Where(m => m.ChatId == chat.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
                
            var unreadCount = await _context.Messages
                .CountAsync(m => m.ChatId == chat.Id && 
                           m.SenderId != userId && 
                           m.ReadAt == null);

            var chatDto = new ChatDto
            {
                Id = chat.Id,
                Name = chat.IsGroup ? chat.Name : GetPrivateChatName(chat, userId),
                IsGroup = chat.IsGroup,
                Description = chat.Description,
                GroupPicture = chat.GroupPicture,
                LastActivity = chat.LastActivity,
                UnreadCount = unreadCount,
                Participants = chat.Participants.Select(p => new UserDto
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                    Name = p.User.Name,
                    ProfilePicture = p.User.ProfilePicture,
                    IsOnline = p.User.IsOnline,
                    LastSeen = p.User.LastSeen
                }).ToList()
            };

            if (lastMessage != null)
            {
                chatDto.LastMessage = MapMessageToDto(lastMessage);
            }

            result.Add(chatDto);
        }

        return result;
    }

    public async Task<ChatDto?> GetChatByIdAsync(int chatId, int userId)
    {
        var chat = await _context.Chats
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null || !chat.Participants.Any(p => p.UserId == userId))
            return null;

        var unreadCount = await _context.Messages
            .CountAsync(m => m.ChatId == chatId && 
                       m.SenderId != userId && 
                       m.ReadAt == null);

        return new ChatDto
        {
            Id = chat.Id,
            Name = chat.IsGroup ? chat.Name : GetPrivateChatName(chat, userId),
            IsGroup = chat.IsGroup,
            Description = chat.Description,
            GroupPicture = chat.GroupPicture,
            LastActivity = chat.LastActivity,
            UnreadCount = unreadCount,
            Participants = chat.Participants.Select(p => new UserDto
            {
                Id = p.User.Id,
                Username = p.User.Username,
                Name = p.User.Name,
                ProfilePicture = p.User.ProfilePicture,
                IsOnline = p.User.IsOnline,
                LastSeen = p.User.LastSeen
            }).ToList()
        };
    }

    public async Task<ChatDto?> CreateChatAsync(CreateChatDto createChatDto, int createdById)
    {
        var chat = new Chat
        {
            Name = createChatDto.Name,
            IsGroup = createChatDto.IsGroup,
            Description = createChatDto.Description,
            CreatedById = createdById
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        var creatorParticipant = new ChatParticipant
        {
            UserId = createdById,
            ChatId = chat.Id,
            Role = ParticipantRole.Owner,
            JoinedAt = DateTime.UtcNow
        };
        _context.ChatParticipants.Add(creatorParticipant);

        if (createChatDto.ParticipantIds != null)
        {
            foreach (var participantId in createChatDto.ParticipantIds)
            {
                if (participantId != createdById)
                {
                    var participant = new ChatParticipant
                    {
                        UserId = participantId,
                        ChatId = chat.Id,
                        Role = ParticipantRole.Member
                    };
                    _context.ChatParticipants.Add(participant);
                }
            }
        }

        await _context.SaveChangesAsync();

        return await GetChatByIdAsync(chat.Id, createdById);
    }

    public async Task<ChatDto?> GetOrCreatePrivateChatAsync(int userId1, int userId2)
    {
        var existingChat = await _context.Chats
            .Where(c => !c.IsGroup)
            .Where(c => c.Participants.Count() == 2 &&
                       c.Participants.Any(cp => cp.UserId == userId1) &&
                       c.Participants.Any(cp => cp.UserId == userId2))
            .FirstOrDefaultAsync();

        if (existingChat != null)
        {
            return await GetChatByIdAsync(existingChat.Id, userId1);
        }

        var chat = new Chat
        {
            Name = "Private Chat",
            IsGroup = false,
            CreatedById = userId1,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        var participants = new[]
        {
            new ChatParticipant { UserId = userId1, ChatId = chat.Id, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow },
            new ChatParticipant { UserId = userId2, ChatId = chat.Id, Role = ParticipantRole.Member, JoinedAt = DateTime.UtcNow }
        };

        _context.ChatParticipants.AddRange(participants);
        await _context.SaveChangesAsync();

        return await GetChatByIdAsync(chat.Id, userId1);
    }

    public async Task<List<MessageDto>> GetChatMessagesAsync(int chatId, int userId, int page = 0, int pageSize = 50)
    {
        var isParticipant = await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

        if (!isParticipant) return new List<MessageDto>();

        var messages = await _context.Messages
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
            .OrderByDescending(m => m.SentAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapMessageToDto).Reverse().ToList();
    }

    public async Task<MessageDto?> SendMessageAsync(SendMessageDto sendMessageDto, int senderId)
    {
        var isParticipant = await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatId == sendMessageDto.ChatId && cp.UserId == senderId);

        if (!isParticipant) return null;

        var message = new Message
        {
            Content = sendMessageDto.Content,
            Type = sendMessageDto.Type,
            SenderId = senderId,
            ChatId = sendMessageDto.ChatId,
            ReplyToMessageId = sendMessageDto.ReplyToMessageId
        };

        _context.Messages.Add(message);

        var chat = await _context.Chats.FindAsync(sendMessageDto.ChatId);
        if (chat != null)
        {
            chat.LastActivity = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var savedMessage = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        return savedMessage != null ? MapMessageToDto(savedMessage) : null;
    }

    public async Task<bool> MarkMessagesAsReadAsync(int chatId, int userId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && m.ReadAt == null)
            .ToListAsync();

        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.ReadAt = DateTime.UtcNow;
                message.Status = MessageStatus.Read;
            }

            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> AddParticipantAsync(int chatId, int userId, int addedByUserId)
    {
        var adderRole = await _context.ChatParticipants
            .Where(cp => cp.ChatId == chatId && cp.UserId == addedByUserId)
            .Select(cp => cp.Role)
            .FirstOrDefaultAsync();

        if (adderRole != ParticipantRole.Admin && adderRole != ParticipantRole.Owner)
            return false;

        var exists = await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

        if (exists) return false;

        var participant = new ChatParticipant
        {
            UserId = userId,
            ChatId = chatId,
            Role = ParticipantRole.Member
        };

        _context.ChatParticipants.Add(participant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveParticipantAsync(int chatId, int userId, int removedByUserId)
    {
        var participant = await _context.ChatParticipants
            .FirstOrDefaultAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

        if (participant == null) return false;

        var removerRole = await _context.ChatParticipants
            .Where(cp => cp.ChatId == chatId && cp.UserId == removedByUserId)
            .Select(cp => cp.Role)
            .FirstOrDefaultAsync();

        if (removerRole == ParticipantRole.Owner ||
            (removerRole == ParticipantRole.Admin && participant.Role == ParticipantRole.Member) ||
            userId == removedByUserId)
        {
            _context.ChatParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateChatAsync(int chatId, string name, string? description, int updatedByUserId)
    {
        var chat = await _context.Chats.FindAsync(chatId);
        if (chat == null) return false;

        var userRole = await _context.ChatParticipants
            .Where(cp => cp.ChatId == chatId && cp.UserId == updatedByUserId)
            .Select(cp => cp.Role)
            .FirstOrDefaultAsync();

        if (userRole != ParticipantRole.Admin && userRole != ParticipantRole.Owner)
            return false;

        chat.Name = name;
        chat.Description = description;

        await _context.SaveChangesAsync();
        return true;
    }

    private static string GetPrivateChatName(Chat chat, int currentUserId)
    {
        var otherUser = chat.Participants.FirstOrDefault(p => p.UserId != currentUserId)?.User;
        return otherUser?.Name ?? "Unknown User";
    }

    private static MessageDto MapMessageToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            Type = message.Type,
            Status = message.Status,
            SenderId = message.SenderId,
            SenderName = message.Sender.Name,
            SenderProfilePicture = message.Sender.ProfilePicture,
            ChatId = message.ChatId,
            ReplyToMessageId = message.ReplyToMessageId,
            ReplyToContent = message.ReplyToMessage?.Content,
            ReplyToSenderName = message.ReplyToMessage?.Sender?.Name,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            IsEdited = message.IsEdited,
            FileName = message.FileName,
            FileSize = message.FileSize
        };
    }
}
