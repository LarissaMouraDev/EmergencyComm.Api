// IDeviceService.cs
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;

namespace EmergencyComm.Api.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAllDevicesAsync();
        Task<Device?> GetDeviceByIdAsync(string id);
        Task<Device> RegisterDeviceAsync(RegisterDeviceDto registerDeviceDto);
        Task UpdateDeviceLocationAsync(string deviceId, LocationUpdateDto locationUpdate);
        Task UpdateDeviceAvailabilityAsync(string deviceId, AvailabilityUpdateDto availabilityUpdate);
        Task<IEnumerable<Device>> GetNearbyDevicesAsync(double latitude, double longitude, double radiusKm);
        Task<IEnumerable<Device>> GetOnlineDevicesAsync();
        Task<IEnumerable<Device>> GetDevicesByStatusAsync(string status);
        Task<bool> RemoveDeviceAsync(string id);
        Task UpdateLastSeenAsync(string deviceId);
        Task<int> GetOnlineDeviceCountAsync();
        Task<IEnumerable<Device>> GetDevicesByTypeAsync(string deviceType);
        Task CleanupOfflineDevicesAsync(int hoursThreshold = 24);
    }
}