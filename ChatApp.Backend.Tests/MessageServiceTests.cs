using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using ChatApp.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChatApp.Backend.Tests
{
    public class MessageServiceTests
    {
        private readonly ChatDbContext _context;
        private readonly Mock<ISentimentAnalysisService> _sentimentAnalysisMock;
        private readonly Mock<ILogger<MessageService>> _loggerMock;
        private readonly MessageService _messageService;

        public MessageServiceTests()
        {
            var options = new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ChatDbContext(options);
            _sentimentAnalysisMock = new Mock<ISentimentAnalysisService>();
            _loggerMock = new Mock<ILogger<MessageService>>();

            _messageService = new MessageService(_context, _loggerMock.Object, _sentimentAnalysisMock.Object);
        }

        [Fact]
        public async Task SaveMessage_ValidMessage_SavesSuccessfully()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = "testuser",
                Message = "Hello, world!",
                Timestamp = DateTime.UtcNow
            };

            _sentimentAnalysisMock.Setup(x => x.AnalyzeSentimentAsync(It.IsAny<string>()))
                .ReturnsAsync(new SentimentResult
                {
                    Sentiment = "Positive",
                    ConfidenceScore = 0.8,
                    Color = "#28a745"
                });

            // Act
            var result = await _messageService.SaveMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(message.Username, result.Username);
            Assert.Equal(message.Message, result.Message);
            Assert.Equal("Positive", result.Sentiment);
            Assert.Equal(0.8, result.SentimentScore);
            Assert.Equal("#28a745", result.SentimentColor);
        }

        [Fact]
        public async Task GetRecentMessages_ReturnsCorrectCount()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new() { Username = "user1", Message = "Message 1", Timestamp = DateTime.UtcNow.AddMinutes(-2) },
                new() { Username = "user2", Message = "Message 2", Timestamp = DateTime.UtcNow.AddMinutes(-1) },
                new() { Username = "user1", Message = "Message 3", Timestamp = DateTime.UtcNow }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.GetRecentMessagesAsync(2);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Message 2", result.First().Message);
            Assert.Equal("Message 3", result.Last().Message);
        }

        [Fact]
        public async Task GetMessagesByUser_ReturnsCorrectMessages()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new() { Username = "user1", Message = "Message 1", Timestamp = DateTime.UtcNow.AddMinutes(-2) },
                new() { Username = "user2", Message = "Message 2", Timestamp = DateTime.UtcNow.AddMinutes(-1) },
                new() { Username = "user1", Message = "Message 3", Timestamp = DateTime.UtcNow }
            };

            await _context.ChatMessages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.GetMessagesByUserAsync("user1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, m => Assert.Equal("user1", m.Username));
        }

        [Fact]
        public async Task UpdateMessage_ValidMessage_UpdatesSuccessfully()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = "testuser",
                Message = "Original message",
                Timestamp = DateTime.UtcNow
            };

            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            _sentimentAnalysisMock.Setup(x => x.AnalyzeSentimentAsync(It.IsAny<string>()))
                .ReturnsAsync(new SentimentResult
                {
                    Sentiment = "Negative",
                    ConfidenceScore = 0.7,
                    Color = "#dc3545"
                });

            // Act
            var result = await _messageService.UpdateMessageAsync(message.Id, "Updated message");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated message", result.Message);
            Assert.True(result.IsEdited);
            Assert.NotNull(result.LastEditedAt);
            Assert.Equal("Negative", result.Sentiment);
            Assert.Equal(0.7, result.SentimentScore);
            Assert.Equal("#dc3545", result.SentimentColor);
        }

        [Fact]
        public async Task DeleteMessage_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var message = new ChatMessage
            {
                Username = "testuser",
                Message = "Message to delete",
                Timestamp = DateTime.UtcNow
            };

            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.DeleteMessageAsync(message.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.ChatMessages.FindAsync(message.Id));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 