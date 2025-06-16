using ChatApp.Backend.Models;
using ChatApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ChatApp.Backend.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat functionality
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public ChatHub(
            IMessageService messageService,
            ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Called when a user joins the chat
        /// </summary>
        public async Task JoinChat(string username)
        {
            try
            {
                // Validate username
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new HubException("Username cannot be empty");
                }

                // Check if username is already taken
                if (_userConnections.Values.Contains(username))
                {
                    throw new HubException("Username is already taken");
                }

                // Store user connection
                _userConnections.TryAdd(Context.ConnectionId, username);

                // Add user to group
                await Groups.AddToGroupAsync(Context.ConnectionId, "ChatUsers");

                // Notify others
                await Clients.Others.SendAsync("UserJoined", username);

                // Send recent messages
                var recentMessages = await _messageService.GetRecentMessagesAsync();
                await Clients.Caller.SendAsync("LoadRecentMessages", recentMessages);

                // Send online users list
                await SendOnlineUsers();

                _logger.LogInformation("User {Username} joined the chat", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinChat for user {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Called when a user sends a message
        /// </summary>
        public async Task SendMessage(string message)
        {
            try
            {
                if (!_userConnections.TryGetValue(Context.ConnectionId, out string? username))
                {
                    throw new HubException("User not found");
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new HubException("Message cannot be empty");
                }

                var chatMessage = new ChatMessage
                {
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()
                };

                var savedMessage = await _messageService.SaveMessageAsync(chatMessage);

                await Clients.All.SendAsync("ReceiveMessage", savedMessage);

                _logger.LogInformation(
                    "Message sent by {Username}. Sentiment: {Sentiment}",
                    username,
                    savedMessage.Sentiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage");
                throw;
            }
        }

        /// <summary>
        /// Called when a user leaves the chat
        /// </summary>
        public async Task LeaveChat()
        {
            try
            {
                if (_userConnections.TryRemove(Context.ConnectionId, out string? username))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatUsers");
                    await Clients.Others.SendAsync("UserLeft", username);
                    await SendOnlineUsers();

                    _logger.LogInformation("User {Username} left the chat", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveChat");
                throw;
            }
        }

        /// <summary>
        /// Called when a user starts typing
        /// </summary>
        public async Task UserTyping()
        {
            try
            {
                if (_userConnections.TryGetValue(Context.ConnectionId, out string? username))
                {
                    await Clients.Others.SendAsync("UserTyping", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserTyping");
                throw;
            }
        }

        /// <summary>
        /// Called when a user stops typing
        /// </summary>
        public async Task UserStoppedTyping()
        {
            try
            {
                if (_userConnections.TryGetValue(Context.ConnectionId, out string? username))
                {
                    await Clients.Others.SendAsync("UserStoppedTyping", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserStoppedTyping");
                throw;
            }
        }

        /// <summary>
        /// Called when a user requests the list of online users
        /// </summary>
        public async Task GetOnlineUsers()
        {
            try
            {
                await SendOnlineUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOnlineUsers");
                throw;
            }
        }

        /// <summary>
        /// Called when the connection is closed
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (_userConnections.TryRemove(Context.ConnectionId, out string? username))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatUsers");
                    await Clients.Others.SendAsync("UserLeft", username);
                    await SendOnlineUsers();

                    _logger.LogInformation("User {Username} disconnected", username);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
                throw;
            }
        }

        private async Task SendOnlineUsers()
        {
            var onlineUsers = _userConnections.Values.Distinct().ToList();
            await Clients.All.SendAsync("OnlineUsers", onlineUsers);
        }
    }
} 