using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Backend.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(RegisterRequest request);
        Task<bool> ValidateUserAsync(string username, string password);
        Task UpdateUserLastLoginAsync(int userId);
        Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    }

    public class UserService : IUserService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ChatDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(RegisterRequest request)
        {
            if (await GetUserByUsernameAsync(request.Username) != null)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            if (await GetUserByEmailAsync(request.Email) != null)
            {
                throw new InvalidOperationException("Email is already registered");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {Username}", user.Username);
            return user;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task UpdateUserLastLoginAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                        u.RefreshTokenExpiryTime > DateTime.UtcNow);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
} 