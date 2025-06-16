using ChatApp.Backend.Models;
using ChatApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Backend.Controllers
{
    /// <summary>
    /// Controller for managing chat messages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(
            IMessageService messageService,
            ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Gets recent messages
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChatMessage>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetRecentMessages([FromQuery] int count = 50)
        {
            try
            {
                var messages = await _messageService.GetRecentMessagesAsync(count);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent messages");
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Gets messages by user
        /// </summary>
        [HttpGet("user/{username}")]
        [ProducesResponseType(typeof(IEnumerable<ChatMessage>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessagesByUser(
            string username,
            [FromQuery] int count = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest("Username is required");
                }

                var messages = await _messageService.GetMessagesByUserAsync(username, count);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for user {Username}", username);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Gets total message count
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetMessageCount()
        {
            try
            {
                var count = await _messageService.GetTotalMessageCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message count");
                return StatusCode(500, "An error occurred while retrieving message count");
            }
        }

        /// <summary>
        /// Updates a message
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ChatMessage), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ChatMessage>> UpdateMessage(
            int id,
            [FromBody] string newMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newMessage))
                {
                    return BadRequest("Message cannot be empty");
                }

                var updatedMessage = await _messageService.UpdateMessageAsync(id, newMessage);
                if (updatedMessage == null)
                {
                    return NotFound($"Message with ID {id} not found");
                }

                return Ok(updatedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message {Id}", id);
                return StatusCode(500, "An error occurred while updating the message");
            }
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            try
            {
                var success = await _messageService.DeleteMessageAsync(id);
                if (!success)
                {
                    return NotFound($"Message with ID {id} not found");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {Id}", id);
                return StatusCode(500, "An error occurred while deleting the message");
            }
        }
    }
} 