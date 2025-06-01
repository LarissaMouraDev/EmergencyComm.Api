using Microsoft.AspNetCore.Mvc;
using EmergencyComm.Api.Services;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Models;

namespace EmergencyComm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all messages
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            try
            {
                var messages = await _messageService.GetAllMessagesAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get message by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            try
            {
                var message = await _messageService.GetMessageByIdAsync(id);
                if (message == null)
                {
                    return NotFound($"Message with ID {id} not found");
                }
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new message
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            try
            {
                var message = await _messageService.CreateMessageAsync(createMessageDto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update message status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateMessageStatus(int id, [FromBody] StatusUpdateDto statusUpdate)
        {
            try
            {
                await _messageService.UpdateMessageStatusAsync(id, statusUpdate.Status);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message status {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var success = await _messageService.DeleteMessageAsync(id);
                if (!success)
                {
                    return NotFound($"Message with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get messages by device ID
        /// </summary>
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByDevice(string deviceId)
        {
            try
            {
                var messages = await _messageService.GetMessagesByDeviceAsync(deviceId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for device {DeviceId}", deviceId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get emergency broadcasts
        /// </summary>
        [HttpGet("emergency")]
        public async Task<ActionResult<IEnumerable<Message>>> GetEmergencyMessages()
        {
            try
            {
                var messages = await _messageService.GetEmergencyMessagesAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency messages");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}