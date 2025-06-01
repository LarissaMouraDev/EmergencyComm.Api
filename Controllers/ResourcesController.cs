using Microsoft.AspNetCore.Mvc;
using EmergencyComm.Api.Services;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Models;

namespace EmergencyComm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger<ResourcesController> _logger;

        public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available resources
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
            try
            {
                var resources = await _resourceService.GetAllResourcesAsync();
                return Ok(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resources");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get resource by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Resource>> GetResource(int id)
        {
            try
            {
                var resource = await _resourceService.GetResourceByIdAsync(id);
                if (resource == null)
                {
                    return NotFound($"Resource with ID {id} not found");
                }
                return Ok(resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource {ResourceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new resource
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Resource>> CreateResource([FromBody] CreateResourceDto createResourceDto)
        {
            try
            {
                var resource = await _resourceService.CreateResourceAsync(createResourceDto);
                return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resource");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update resource information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResource(int id, [FromBody] CreateResourceDto updateResourceDto)
        {
            try
            {
                await _resourceService.UpdateResourceAsync(id, updateResourceDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resource {ResourceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a resource
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            try
            {
                var success = await _resourceService.DeleteResourceAsync(id);
                if (!success)
                {
                    return NotFound($"Resource with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resource {ResourceId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get resources by type
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResourcesByType(string type)
        {
            try
            {
                var resources = await _resourceService.GetResourcesByTypeAsync(type);
                return Ok(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resources by type {Type}", type);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get nearby resources
        /// </summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetNearbyResources([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 10.0)
        {
            try
            {
                var resources = await _resourceService.GetNearbyResourcesAsync(latitude, longitude, radiusKm);
                return Ok(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby resources");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get available resources only
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetAvailableResources()
        {
            try
            {
                var resources = await _resourceService.GetAvailableResourcesAsync();
                return Ok(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available resources");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search resources by name or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Resource>>> SearchResources([FromQuery] string query)
        {
            try
            {
                var resources = await _resourceService.SearchResourcesAsync(query);
                return Ok(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching resources with query {Query}", query);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}