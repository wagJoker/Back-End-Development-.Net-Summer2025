using Azure;
using Azure.AI.TextAnalytics;
using ChatApp.Backend.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatApp.Backend.Services
{
    /// <summary>
    /// Service for analyzing sentiment of text using Azure Cognitive Services
    /// </summary>
    public interface ISentimentAnalysisService
    {
        /// <summary>
        /// Analyzes the sentiment of the provided text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Sentiment analysis result</returns>
        Task<SentimentResult> AnalyzeSentimentAsync(string text);
    }

    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly TextAnalyticsClient _client;
        private readonly ILogger<SentimentAnalysisService> _logger;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public SentimentAnalysisService(
            IConfiguration configuration,
            ILogger<SentimentAnalysisService> logger,
            IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            
            var endpoint = configuration["CognitiveServices:TextAnalyticsEndpoint"];
            var key = configuration["CognitiveServices:TextAnalyticsKey"];
            
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Text Analytics configuration is missing. Sentiment analysis will return neutral results.");
                return;
            }
            
            _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
            
            // Configure cache options
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24))
                .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                .SetPriority(CacheItemPriority.Normal);
        }

        public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return GetNeutralResult();
            }

            // Check cache first
            var cacheKey = $"sentiment_{text.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out SentimentResult? cachedResult))
            {
                _logger.LogDebug("Retrieved sentiment from cache for text: {Text}", text);
                return cachedResult!;
            }

            if (_client == null)
            {
                return GetNeutralResult();
            }

            try
            {
                var response = await _client.AnalyzeSentimentAsync(text);
                var sentiment = response.Value;
                
                var result = new SentimentResult
                {
                    Sentiment = sentiment.Sentiment.ToString(),
                    ConfidenceScore = GetHighestConfidenceScore(sentiment),
                    Color = GetSentimentColor(sentiment.Sentiment.ToString())
                };

                // Cache the result
                _cache.Set(cacheKey, result, _cacheOptions);
                
                _logger.LogInformation(
                    "Analyzed sentiment for text: {Text}. Result: {Sentiment} ({Score})",
                    text,
                    result.Sentiment,
                    result.ConfidenceScore);

                return result;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Azure Text Analytics request failed for text: {Text}", text);
                return GetNeutralResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment for text: {Text}", text);
                return GetNeutralResult();
            }
        }

        private static double GetHighestConfidenceScore(DocumentSentiment sentiment)
        {
            return Math.Max(
                Math.Max(sentiment.ConfidenceScores.Positive, sentiment.ConfidenceScores.Negative),
                sentiment.ConfidenceScores.Neutral);
        }

        private static string GetSentimentColor(string sentiment)
        {
            return sentiment.ToLower() switch
            {
                "positive" => "#28a745", // Bootstrap success color
                "negative" => "#dc3545", // Bootstrap danger color
                "neutral" => "#6c757d",  // Bootstrap secondary color
                _ => "#6c757d"
            };
        }

        private static SentimentResult GetNeutralResult()
        {
            return new SentimentResult
            {
                Sentiment = "Neutral",
                ConfidenceScore = 0.5,
                Color = "#6c757d"
            };
        }
    }

    /// <summary>
    /// Result of sentiment analysis
    /// </summary>
    public class SentimentResult
    {
        /// <summary>
        /// The detected sentiment (Positive, Negative, Neutral)
        /// </summary>
        public string Sentiment { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score of the sentiment analysis (0-1)
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Color code for sentiment visualization
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }
} 