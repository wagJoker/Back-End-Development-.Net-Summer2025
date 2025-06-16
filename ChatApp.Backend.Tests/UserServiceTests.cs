using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using ChatApp.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChatApp.Backend.Tests
{
    public class UserServiceTests
    {
        private readonly ChatDbContext _context;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ChatDbContext(options);
            _loggerMock = new Mock<ILogger<UserService>>();

            _userService = new UserService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_CreatesSuccessfully()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Username, result.Username);
            Assert.Equal(request.Email, result.Email);
            Assert.NotNull(result.PasswordHash);
            Assert.True(result.IsActive);
            Assert.NotNull(result.CreatedAt);
        }

        [Fact]
        public async Task CreateUser_DuplicateUsername_ThrowsException()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "testuser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "new@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(request));
        }

        [Fact]
        public async Task ValidateUser_ValidCredentials_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", // hash of "password123"
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.ValidateUserAsync("testuser", "password123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateUser_InvalidCredentials_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", // hash of "password123"
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.ValidateUserAsync("testuser", "wrongpassword");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateRefreshToken_ValidUser_UpdatesSuccessfully()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var refreshToken = "new-refresh-token";
            var expiryTime = DateTime.UtcNow.AddDays(7);

            // Act
            await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, expiryTime);

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal(refreshToken, updatedUser.RefreshToken);
            Assert.Equal(expiryTime, updatedUser.RefreshTokenExpiryTime);
        }

        [Fact]
        public async Task GetUserByRefreshToken_ValidToken_ReturnsUser()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetUserByRefreshToken_ExpiredToken_ReturnsNull()
        {
            // Arrange
            var refreshToken = "expired-refresh-token";
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 