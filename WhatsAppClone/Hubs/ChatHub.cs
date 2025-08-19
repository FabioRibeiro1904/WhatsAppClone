using Microsoft.AspNetCore.SignalR;
using WhatsAppClone.DTOs;
using WhatsAppClone.Services;
using System.Collections.Concurrent;

namespace WhatsAppClone.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;
    private static readonly ConcurrentDictionary<string, int> _userConnections = new();
    private static readonly ConcurrentDictionary<int, HashSet<string>> _userConnectionIds = new();
    private static readonly ConcurrentDictionary<string, TypingInfo> _typingUsers = new();

    public ChatHub(IChatService chatService, IUserService userService)
    {
        _chatService = chatService;
        _userService = userService;
    }

    public async Task JoinChat(int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
    }

    public async Task SendMessage(SendMessageDto messageDto)
    {
        if (_userConnections.TryGetValue(Context.ConnectionId, out var senderId))
        {
            var message = await _chatService.SendMessageAsync(messageDto, senderId);
            if (message != null)
            {
                await Clients.Group($"Chat_{messageDto.ChatId}")
                    .SendAsync("ReceiveMessage", message);

                var chat = await _chatService.GetChatByIdAsync(messageDto.ChatId, senderId);
                if (chat != null)
                {
                    foreach (var participant in chat.Participants)
                    {
                        await Clients.Group($"User_{participant.Id}")
                            .SendAsync("ChatUpdated", chat);
                    }
                }
            }
        }
    }

    public async Task StartTyping(int chatId)
    {
        if (_userConnections.TryGetValue(Context.ConnectionId, out var userId))
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                var typingInfo = new TypingInfo
                {
                    ChatId = chatId,
                    UserId = userId,
                    UserName = user.Name,
                    StartTime = DateTime.UtcNow
                };

                _typingUsers.AddOrUpdate(Context.ConnectionId, typingInfo, (key, old) => typingInfo);

                var typingDto = new TypingIndicatorDto
                {
                    ChatId = chatId,
                    UserId = userId,
                    UserName = user.Name,
                    IsTyping = true
                };

                await Clients.GroupExcept($"Chat_{chatId}", Context.ConnectionId)
                    .SendAsync("UserTyping", typingDto);
            }
        }
    }

    public async Task StopTyping(int chatId)
    {
        if (_userConnections.TryGetValue(Context.ConnectionId, out var userId))
        {
            _typingUsers.TryRemove(Context.ConnectionId, out _);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                var typingDto = new TypingIndicatorDto
                {
                    ChatId = chatId,
                    UserId = userId,
                    UserName = user.Name,
                    IsTyping = false
                };

                await Clients.GroupExcept($"Chat_{chatId}", Context.ConnectionId)
                    .SendAsync("UserTyping", typingDto);
            }
        }
    }

    public async Task MarkMessagesAsRead(int chatId)
    {
        try
        {
            if (_userConnections.TryGetValue(Context.ConnectionId, out var userId))
            {
                await _chatService.MarkMessagesAsReadAsync(chatId, userId);
                
                await Clients.GroupExcept($"Chat_{chatId}", Context.ConnectionId)
                    .SendAsync("MessagesRead", new { ChatId = chatId, UserId = userId });
                
                await Clients.Caller.SendAsync("MessagesRead", new { ChatId = chatId, UserId = userId });
            }
        }
        catch
        {
            // Ignore mark as read errors
        }
    }

    public async Task JoinUserGroup(int userId)
    {
        _userConnections[Context.ConnectionId] = userId;
        
        if (!_userConnectionIds.ContainsKey(userId))
        {
            _userConnectionIds[userId] = new HashSet<string>();
        }
        _userConnectionIds[userId].Add(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        
        await _userService.UpdateOnlineStatusAsync(userId, true);
        
        await Clients.All.SendAsync("UserOnlineStatusChanged", new { UserId = userId, IsOnline = true });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userConnections.TryRemove(Context.ConnectionId, out var userId))
        {
            if (_userConnectionIds.TryGetValue(userId, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                
                if (connections.Count == 0)
                {
                    _userConnectionIds.TryRemove(userId, out _);
                    await _userService.UpdateOnlineStatusAsync(userId, false);
                    
                    await Clients.All.SendAsync("UserOnlineStatusChanged", new { UserId = userId, IsOnline = false });
                }
            }

            if (_typingUsers.TryRemove(Context.ConnectionId, out var typingInfo))
            {
                var typingDto = new TypingIndicatorDto
                {
                    ChatId = typingInfo.ChatId,
                    UserId = typingInfo.UserId,
                    UserName = typingInfo.UserName,
                    IsTyping = false
                };

                await Clients.GroupExcept($"Chat_{typingInfo.ChatId}", Context.ConnectionId)
                    .SendAsync("UserTyping", typingDto);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static void CleanupTypingIndicators()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-10);
        var keysToRemove = _typingUsers
            .Where(kv => kv.Value.StartTime < cutoff)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _typingUsers.TryRemove(key, out _);
        }
    }
}

public class TypingInfo
{
    public int ChatId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
}
