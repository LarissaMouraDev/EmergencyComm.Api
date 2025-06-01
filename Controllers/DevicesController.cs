using Microsoft.AspNetCore.Mvc;
using EmergencyComm.Api.Services;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Models;

namespace EmergencyComm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all devices in the network
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
        {
            try
            {
                var devices = await _deviceService.GetAllDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(string id)
        {
            try
            {
                var device = await _deviceService.GetDeviceByIdAsync(id);
                if (device == null)
                {
                    return NotFound($"Device with ID {id} not found");
                }
                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device {DeviceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Register a new device in the network
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<Device>> RegisterDevice([FromBody] RegisterDeviceDto registerDeviceDto)
        {
            try
            {
                var device = await _deviceService.RegisterDeviceAsync(registerDeviceDto);
                return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update device location
        /// </summary>
        [HttpPut("{id}/location")]
        public async Task<IActionResult> UpdateLocation(string id, [FromBody] LocationUpdateDto locationUpdate)
        {
            try
            {
                await _deviceService.UpdateDeviceLocationAsync(id, locationUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device location {DeviceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update device availability status
        /// </summary>
        [HttpPut("{id}/availability")]
        public async Task<IActionResult> UpdateAvailability(string id, [FromBody] AvailabilityUpdateDto availabilityUpdate)
        {
            try
            {
                await _deviceService.UpdateDeviceAvailabilityAsync(id, availabilityUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device availability {DeviceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get nearby devices
        /// </summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<Device>>> GetNearbyDevices([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 5.0)
        {
            try
            {
                var devices = await _deviceService.GetNearbyDevicesAsync(latitude, longitude, radiusKm);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby devices");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get online devices
        /// </summary>
        [HttpGet("online")]
        public async Task<ActionResult<IEnumerable<Device>>> GetOnlineDevices()
        {
            try
            {
                var devices = await _deviceService.GetOnlineDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving online devices");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove device from network
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDevice(string id)
        {
            try
            {
                var success = await _deviceService.RemoveDeviceAsync(id);
                if (!success)
                {
                    return NotFound($"Device with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device {DeviceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}