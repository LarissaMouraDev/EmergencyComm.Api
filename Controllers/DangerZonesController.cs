using Microsoft.AspNetCore.Mvc;
using EmergencyComm.Api.Data;
using EmergencyComm.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmergencyComm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DangerZonesController : ControllerBase
    {
        private readonly EmergencyContext _context;
        private readonly ILogger<DangerZonesController> _logger;

        public DangerZonesController(EmergencyContext context, ILogger<DangerZonesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all danger zones
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DangerZone>>> GetDangerZones()
        {
            try
            {
                var dangerZones = await _context.DangerZones
                    .Where(dz => dz.IsActive)
                    .OrderByDescending(dz => dz.CreatedAt)
                    .ToListAsync();
                return Ok(dangerZones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving danger zones");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get danger zone by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DangerZone>> GetDangerZone(int id)
        {
            try
            {
                var dangerZone = await _context.DangerZones.FindAsync(id);
                if (dangerZone == null)
                {
                    return NotFound($"Danger zone with ID {id} not found");
                }
                return Ok(dangerZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving danger zone {DangerZoneId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new danger zone
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DangerZone>> CreateDangerZone([FromBody] DangerZone dangerZone)
        {
            try
            {
                dangerZone.CreatedAt = DateTime.UtcNow;
                dangerZone.IsActive = true;

                _context.DangerZones.Add(dangerZone);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDangerZone), new { id = dangerZone.Id }, dangerZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating danger zone");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update danger zone
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDangerZone(int id, [FromBody] DangerZone dangerZone)
        {
            if (id != dangerZone.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _context.Entry(dangerZone).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DangerZoneExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating danger zone {DangerZoneId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deactivate danger zone
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateDangerZone(int id)
        {
            try
            {
                var dangerZone = await _context.DangerZones.FindAsync(id);
                if (dangerZone == null)
                {
                    return NotFound($"Danger zone with ID {id} not found");
                }

                dangerZone.IsActive = false;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating danger zone {DangerZoneId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get danger zones near location
        /// </summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<DangerZone>>> GetNearbyDangerZones([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 5.0)
        {
            try
            {
                // Simple distance calculation - for production use proper geospatial queries
                var dangerZones = await _context.DangerZones
                    .Where(dz => dz.IsActive)
                    .ToListAsync();

                var nearbyZones = dangerZones.Where(dz =>
                {
                    var distance = CalculateDistance(latitude, longitude, dz.Latitude, dz.Longitude);
                    return distance <= radiusKm;
                }).ToList();

                return Ok(nearbyZones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby danger zones");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get danger zones by severity level
        /// </summary>
        [HttpGet("severity/{level}")]
        public async Task<ActionResult<IEnumerable<DangerZone>>> GetDangerZonesBySeverity(string level)
        {
            try
            {
                var dangerZones = await _context.DangerZones
                    .Where(dz => dz.IsActive && dz.SeverityLevel.ToLower() == level.ToLower())
                    .OrderByDescending(dz => dz.CreatedAt)
                    .ToListAsync();

                return Ok(dangerZones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving danger zones by severity {Level}", level);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<bool> DangerZoneExists(int id)
        {
            return await _context.DangerZones.AnyAsync(e => e.Id == id);
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}