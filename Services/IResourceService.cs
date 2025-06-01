// IResourceService.cs
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;

namespace EmergencyComm.Api.Services
{
    public interface IResourceService
    {
        Task<IEnumerable<Resource>> GetAllResourcesAsync();
        Task<Resource?> GetResourceByIdAsync(int id);
        Task<Resource> CreateResourceAsync(CreateResourceDto createResourceDto);
        Task UpdateResourceAsync(int id, CreateResourceDto updateResourceDto);
        Task<bool> DeleteResourceAsync(int id);
        Task<IEnumerable<Resource>> GetResourcesByTypeAsync(string type);
        Task<IEnumerable<Resource>> GetNearbyResourcesAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Resource>> GetAvailableResourcesAsync();
        Task<IEnumerable<Resource>> GetResourcesByProviderAsync(string providerId);
        Task<IEnumerable<Resource>> SearchResourcesAsync(string query);
        Task<IEnumerable<Resource>> GetResourcesByPriorityAsync(string priority);
        Task UpdateResourceAvailabilityAsync(int id, bool isAvailable, string status);
        Task UpdateResourceUsageAsync(int id, int currentUsage);
        Task<IEnumerable<Resource>> GetExpiringResourcesAsync(int hoursFromNow = 24);
    }
}
