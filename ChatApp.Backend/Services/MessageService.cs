using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatApp.Backend.Services
{
    /// <summary>
    /// Service for managing chat messages
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Saves a new message to the database
        /// </summary>
        Task<ChatMessage> SaveMessageAsync(ChatMessage message);

        /// <summary>
        /// Gets recent messages
        /// </summary>
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50);

        /// <summary>
        /// Gets total message count
        /// </summary>
        Task<int> GetTotalMessageCountAsync();

        /// <summary>
        /// Gets messages by user
        /// </summary>
        Task<IEnumerable<ChatMessage>> GetMessagesByUserAsync(string username, int count = 20);

        /// <summary>
        /// Updates an existing message
        /// </summary>
        Task<ChatMessage?> UpdateMessageAsync(int id, string newMessage);

        /// <summary>
        /// Deletes a message
        /// </summary>
        Task<bool> DeleteMessageAsync(int id);
    }

    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly ISentimentAnalysisService _sentimentAnalysis;

        public MessageService(
            ChatDbContext context,
            ILogger<MessageService> logger,
            ISentimentAnalysisService sentimentAnalysis)
        {
            _context = context;
            _logger = logger;
            _sentimentAnalysis = sentimentAnalysis;
        }

        public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
        {
            try
            {
                // Validate message
                if (string.IsNullOrWhiteSpace(message.Message))
                {
                    throw new ArgumentException("Message cannot be empty", nameof(message));
                }

                if (string.IsNullOrWhiteSpace(message.Username))
                {
                    throw new ArgumentException("Username cannot be empty", nameof(message));
                }

                // Analyze sentiment
                var sentimentResult = await _sentimentAnalysis.AnalyzeSentimentAsync(message.Message);
                message.Sentiment = sentimentResult.Sentiment;
                message.SentimentScore = sentimentResult.ConfidenceScore;
                message.SentimentColor = sentimentResult.Color;

                // Save message
                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Message saved successfully. Id: {Id}, User: {Username}, Sentiment: {Sentiment}",
                    message.Id,
                    message.Username,
                    message.Sentiment);

                return message;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error saving message for user {Username}", message.Username);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error saving message for user {Username}", message.Username);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message for user {Username}", message.Username);
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            try
            {
                return await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent messages");
                throw;
            }
        }

        public async Task<int> GetTotalMessageCountAsync()
        {
            try
            {
                return await _context.ChatMessages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total message count");
                return 0;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByUserAsync(string username, int count = 20)
        {
            try
            {
                return await _context.ChatMessages
                    .Where(m => m.Username == username)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for user {Username}", username);
                throw;
            }
        }

        public async Task<ChatMessage?> UpdateMessageAsync(int id, string newMessage)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(id);
                if (message == null)
                {
                    return null;
                }

                message.Message = newMessage;
                message.IsEdited = true;
                message.LastEditedAt = DateTime.UtcNow;

                // Re-analyze sentiment
                var sentimentResult = await _sentimentAnalysis.AnalyzeSentimentAsync(newMessage);
                message.Sentiment = sentimentResult.Sentiment;
                message.SentimentScore = sentimentResult.ConfidenceScore;
                message.SentimentColor = sentimentResult.Color;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Message updated successfully. Id: {Id}, User: {Username}",
                    message.Id,
                    message.Username);

                return message;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating message {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(id);
                if (message == null)
                {
                    return false;
                }

                _context.ChatMessages.Remove(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Message deleted successfully. Id: {Id}, User: {Username}",
                    message.Id,
                    message.Username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {Id}", id);
                throw;
            }
        }
    }
} 