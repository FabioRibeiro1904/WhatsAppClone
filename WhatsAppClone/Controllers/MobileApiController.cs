using Microsoft.AspNetCore.Mvc;
using WhatsAppClone.Services;
using WhatsAppClone.DTOs;

namespace WhatsAppClone.Controllers
{
    [ApiController]
    [Route("api/mobile")]
    public class MobileApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public MobileApiController(IUserService userService, IChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userService.LoginAsync(loginDto.Username, loginDto.Password);
                
                if (user != null)
                {
                    return Ok(new { 
                        success = true, 
                        user = new {
                            id = user.Id,
                            username = user.Username,
                            name = user.Name,
                            email = user.Email,
                            status = user.Status,
                            profilePicture = user.ProfilePicture,
                            isOnline = user.IsOnline,
                            lastSeen = user.LastSeen
                        }
                    });
                }
                
                return Unauthorized(new { success = false, message = "Credenciais inválidas" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("users/{userId}/chats")]
        public async Task<IActionResult> GetUserChats(int userId)
        {
            try
            {
                var chats = await _chatService.GetUserChatsAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("chats/{chatId}/messages")]
        public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] int userId)
        {
            try
            {
                var messages = await _chatService.GetChatMessagesAsync(chatId, userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            try
            {
                var users = await _userService.SearchUsersAsync(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("chats/private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatDto dto)
        {
            try
            {
                var chat = await _chatService.GetOrCreatePrivateChatAsync(dto.UserId1, dto.UserId2);
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class CreatePrivateChatDto
    {
        public int UserId1 { get; set; }
        public int UserId2 { get; set; }
    }
}
