using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Data
{
    /// <summary>
    /// Database context for the chat application
    /// </summary>
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Collection of chat messages
        /// </summary>
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        /// <summary>
        /// Collection of users
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Required fields
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                // Optional fields
                entity.Property(e => e.Sentiment)
                    .HasMaxLength(20);

                entity.Property(e => e.SentimentColor)
                    .HasMaxLength(20);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45); // IPv6 max length

                // Indexes
                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("IX_ChatMessages_Timestamp");

                entity.HasIndex(e => e.Username)
                    .HasDatabaseName("IX_ChatMessages_Username");

                entity.HasIndex(e => new { e.Username, e.Timestamp })
                    .HasDatabaseName("IX_ChatMessages_Username_Timestamp");

                entity.HasIndex(e => e.Sentiment)
                    .HasDatabaseName("IX_ChatMessages_Sentiment");

                // Constraints
                entity.HasCheckConstraint("CK_ChatMessages_SentimentScore", 
                    "[SentimentScore] IS NULL OR ([SentimentScore] >= 0 AND [SentimentScore] <= 1)");

                // Query filters
                entity.HasQueryFilter(e => !e.IsModerated);
            });

            modelBuilder.Entity<User>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Required fields
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(100);

                // Indexes
                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.HasIndex(e => e.RefreshToken)
                    .HasDatabaseName("IX_Users_RefreshToken");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ChatMessage>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Property(p => p.Message).IsModified)
                {
                    entry.Entity.IsEdited = true;
                    entry.Entity.LastEditedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
} 