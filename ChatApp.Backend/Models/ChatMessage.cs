using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChatApp.Backend.Models
{
    /// <summary>
    /// Represents a chat message in the system
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Username of the message sender
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Content of the message
        /// </summary>
        [Required(ErrorMessage = "Message content is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Sentiment analysis result (Positive, Negative, Neutral)
        /// </summary>
        [StringLength(20)]
        public string? Sentiment { get; set; }
        
        /// <summary>
        /// Confidence score of the sentiment analysis (0-1)
        /// </summary>
        [Range(0, 1)]
        public double? SentimentScore { get; set; }
        
        /// <summary>
        /// Color code for sentiment visualization
        /// </summary>
        [StringLength(20)]
        public string? SentimentColor { get; set; }

        /// <summary>
        /// IP address of the sender (for moderation purposes)
        /// </summary>
        [JsonIgnore]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Whether the message has been moderated
        /// </summary>
        public bool IsModerated { get; set; }

        /// <summary>
        /// Whether the message has been edited
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// When the message was last edited
        /// </summary>
        public DateTime? LastEditedAt { get; set; }

        /// <summary>
        /// Version of the message for optimistic concurrency
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
} 