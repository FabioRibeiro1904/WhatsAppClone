using Microsoft.EntityFrameworkCore;
using WhatsAppClone.Data;
using WhatsAppClone.DTOs;
using WhatsAppClone.Models;
using BCrypt.Net;

namespace WhatsAppClone.Services;

public interface IUserService
{
    Task<UserDto?> AuthenticateAsync(LoginDto loginDto);
    Task<UserDto?> RegisterAsync(RegisterDto registerDto);
    Task<UserDto?> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string name, string email, string password);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<List<UserDto>> SearchUsersAsync(string searchTerm);
    Task<bool> UpdateUserAsync(int userId, UpdateProfileDto updateDto);
    Task<bool> UpdateOnlineStatusAsync(int userId, bool isOnline);
    Task<List<UserDto>> GetOnlineUsersAsync();
}

public class UserService : IUserService
{
    private readonly WhatsAppDbContext _context;

    public UserService(WhatsAppDbContext context)
    {
        _context = context;
    }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user != null)
            {
                try 
                {
                    var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                    
                    if (passwordValid)
                    {
                        return user;
                    }
                }
                catch
                {
                    // Ignore password verification errors
                }
            }
            
            return null;
        }

    public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            return null;

        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            return null;

        var user = new User
        {
            Username = registerDto.Username,
            Name = registerDto.Name,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            IsOnline = true,
            LastSeen = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

        public async Task<UserDto?> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user != null)
            {
                try 
                {
                    var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                    
                    if (passwordValid)
                    {
                        return MapToDto(user);
                    }
                }
                catch
                {
                    // Error handling without logging
                }
            }
            
            return null;
        }        public async Task<UserDto?> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await AuthenticateAsync(loginDto.Username, loginDto.Password);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<bool> RegisterAsync(string username, string name, string email, string password)
    {
        
        var usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
        
        if (usernameExists)
        {
            return false;
        }

        var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
        
        if (emailExists)
        {
            return false;
        }

        var user = new User
        {
            Username = username,
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Status = "Olá! Estou usando WhatsApp Clone.",
            IsOnline = false,
            LastSeen = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> SearchUsersAsync(string searchTerm)
    {
        var users = await _context.Users
            .Where(u => u.Username.Contains(searchTerm) || u.Name.Contains(searchTerm))
            .Take(20)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateProfileDto updateDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.Name = updateDto.Name;
        user.Status = updateDto.Status;
        user.ProfilePicture = updateDto.ProfilePicture;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateOnlineStatusAsync(int userId, bool isOnline)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsOnline = isOnline;
        user.LastSeen = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetOnlineUsersAsync()
    {
        var users = await _context.Users
            .Where(u => u.IsOnline)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            ProfilePicture = user.ProfilePicture,
            Status = user.Status,
            IsOnline = user.IsOnline,
            LastSeen = user.LastSeen
        };
    }
}
