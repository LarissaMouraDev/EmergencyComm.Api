// ResourceService.cs
using Microsoft.EntityFrameworkCore;
using EmergencyComm.Api.Data;
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyComm.Api.Services
{
    public class ResourceService : IResourceService
    {
        private readonly EmergencyContext _context;
        private readonly IHubContext<CommunicationHub> _hubContext;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(EmergencyContext context, IHubContext<CommunicationHub> hubContext, ILogger<ResourceService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Resource?> GetResourceByIdAsync(int id)
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Resource> CreateResourceAsync(CreateResourceDto createResourceDto)
        {
            var resource = new Resource
            {
                Name = createResourceDto.Name,
                Type = createResourceDto.Type,
                Description = createResourceDto.Description,
                Latitude = createResourceDto.Latitude,
                Longitude = createResourceDto.Longitude,
                ProviderId = createResourceDto.ProviderId,
                IsAvailable = true,
                Capacity = createResourceDto.Capacity,
                CurrentUsage = createResourceDto.CurrentUsage ?? 0,
                Status = "Available",
                Priority = createResourceDto.Priority,
                ContactName = createResourceDto.ContactName,
                ContactPhone = createResourceDto.ContactPhone,
                AccessInstructions = createResourceDto.AccessInstructions,
                Requirements = createResourceDto.Requirements,
                Restrictions = createResourceDto.Restrictions,
                ExpiresAt = createResourceDto.ExpiresAt,
                AdditionalData = createResourceDto.AdditionalData,
                CreatedAt = DateTime.UtcNow
            };

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            await NotifyNewResource(resource);

            _logger.LogInformation("New resource created: {ResourceId} by {ProviderId}", resource.Id, resource.ProviderId);
            return resource;
        }

        public async Task UpdateResourceAsync(int id, CreateResourceDto updateResourceDto)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                throw new ArgumentException($"Resource with ID {id} not found");
            }

            resource.Name = updateResourceDto.Name;
            resource.Type = updateResourceDto.Type;
            resource.Description = updateResourceDto.Description;
            resource.Latitude = updateResourceDto.Latitude;
            resource.Longitude = updateResourceDto.Longitude;
            resource.Capacity = updateResourceDto.Capacity;
            resource.CurrentUsage = updateResourceDto.CurrentUsage ?? resource.CurrentUsage;
            resource.Priority = updateResourceDto.Priority;
            resource.ContactName = updateResourceDto.ContactName;
            resource.ContactPhone = updateResourceDto.ContactPhone;
            resource.AccessInstructions = updateResourceDto.AccessInstructions;
            resource.Requirements = updateResourceDto.Requirements;
            resource.Restrictions = updateResourceDto.Restrictions;
            resource.ExpiresAt = updateResourceDto.ExpiresAt;
            resource.AdditionalData = updateResourceDto.AdditionalData;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyResourceUpdate(resource);

            _logger.LogInformation("Resource updated: {ResourceId}", id);
        }

        public async Task<bool> DeleteResourceAsync(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return false;

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            await NotifyResourceRemoved(id);

            _logger.LogInformation("Resource deleted: {ResourceId}", id);
            return true;
        }

        public async Task<IEnumerable<Resource>> GetResourcesByTypeAsync(string type)
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.Type.ToLower() == type.ToLower() && r.IsAvailable)
                .OrderBy(r => r.Priority == "Critical" ? 0 : r.Priority == "High" ? 1 : r.Priority == "Normal" ? 2 : 3)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetNearbyResourcesAsync(double latitude, double longitude, double radiusKm)
        {
            var resources = await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.IsAvailable)
                .ToListAsync();

            return resources.Where(r =>
            {
                var distance = CalculateDistance(latitude, longitude, r.Latitude, r.Longitude);
                return distance <= radiusKm;
            }).OrderBy(r =>
            {
                return CalculateDistance(latitude, longitude, r.Latitude, r.Longitude);
            });
        }

        public async Task<IEnumerable<Resource>> GetAvailableResourcesAsync()
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.IsAvailable &&
                           r.Status != "Unavailable" &&
                           (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow))
                .OrderBy(r => r.Priority == "Critical" ? 0 : r.Priority == "High" ? 1 : r.Priority == "Normal" ? 2 : 3)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetResourcesByProviderAsync(string providerId)
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.ProviderId == providerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> SearchResourcesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAvailableResourcesAsync();
            }

            var lowerQuery = query.ToLower();
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.IsAvailable &&
                           (r.Name.ToLower().Contains(lowerQuery) ||
                            r.Description!.ToLower().Contains(lowerQuery) ||
                            r.Type.ToLower().Contains(lowerQuery)))
                .OrderBy(r => r.Priority == "Critical" ? 0 : r.Priority == "High" ? 1 : r.Priority == "Normal" ? 2 : 3)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resource>> GetResourcesByPriorityAsync(string priority)
        {
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.Priority.ToLower() == priority.ToLower() && r.IsAvailable)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateResourceAvailabilityAsync(int id, bool isAvailable, string status)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                throw new ArgumentException($"Resource with ID {id} not found");
            }

            resource.IsAvailable = isAvailable;
            resource.Status = status;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyResourceUpdate(resource);

            _logger.LogInformation("Resource availability updated: {ResourceId} - Available: {IsAvailable}, Status: {Status}",
                id, isAvailable, status);
        }

        public async Task UpdateResourceUsageAsync(int id, int currentUsage)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                throw new ArgumentException($"Resource with ID {id} not found");
            }

            resource.CurrentUsage = currentUsage;
            resource.UpdatedAt = DateTime.UtcNow;

            if (resource.Capacity.HasValue)
            {
                var usagePercentage = (double)currentUsage / resource.Capacity.Value;
                if (usagePercentage >= 1.0)
                {
                    resource.Status = "Full";
                    resource.IsAvailable = false;
                }
                else if (usagePercentage >= 0.8)
                {
                    resource.Status = "Limited";
                }
                else
                {
                    resource.Status = "Available";
                    resource.IsAvailable = true;
                }
            }

            await _context.SaveChangesAsync();
            await NotifyResourceUpdate(resource);

            _logger.LogDebug("Resource usage updated: {ResourceId} - Usage: {CurrentUsage}/{Capacity}",
                id, currentUsage, resource.Capacity);
        }

        public async Task<IEnumerable<Resource>> GetExpiringResourcesAsync(int hoursFromNow = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(hoursFromNow);
            return await _context.Resources
                .Include(r => r.Provider)
                .Where(r => r.ExpiresAt.HasValue &&
                           r.ExpiresAt <= cutoffTime &&
                           r.IsAvailable)
                .OrderBy(r => r.ExpiresAt)
                .ToListAsync();
        }

        private async Task NotifyNewResource(Resource resource)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("NewResource", resource);

                if (resource.Priority == "Critical")
                {
                    await _hubContext.Clients.All.SendAsync("CriticalResourceAvailable", resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying new resource {ResourceId}", resource.Id);
            }
        }

        private async Task NotifyResourceUpdate(Resource resource)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ResourceUpdated", resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying resource update {ResourceId}", resource.Id);
            }
        }

        private async Task NotifyResourceRemoved(int resourceId)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ResourceRemoved", resourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying resource removal {ResourceId}", resourceId);
            }
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
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