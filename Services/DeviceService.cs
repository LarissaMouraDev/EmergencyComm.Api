// DeviceService.cs
using Microsoft.EntityFrameworkCore;
using EmergencyComm.Api.Data;
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyComm.Api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly EmergencyContext _context;
        private readonly IHubContext<CommunicationHub> _hubContext;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(EmergencyContext context, IHubContext<CommunicationHub> hubContext, ILogger<DeviceService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Device>> GetAllDevicesAsync()
        {
            return await _context.Devices
                .OrderByDescending(d => d.LastSeen)
                .ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(string id)
        {
            return await _context.Devices
                .Include(d => d.SentMessages)
                .Include(d => d.SharedResources)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Device> RegisterDeviceAsync(RegisterDeviceDto registerDeviceDto)
        {
            var existingDevice = await _context.Devices.FindAsync(registerDeviceDto.Id);

            if (existingDevice != null)
            {
                existingDevice.Name = registerDeviceDto.Name;
                existingDevice.DeviceType = registerDeviceDto.DeviceType;
                existingDevice.Latitude = registerDeviceDto.Latitude;
                existingDevice.Longitude = registerDeviceDto.Longitude;
                existingDevice.Altitude = registerDeviceDto.Altitude;
                existingDevice.AdditionalInfo = registerDeviceDto.AdditionalInfo;
                existingDevice.BatteryLevel = registerDeviceDto.BatteryLevel;
                existingDevice.NetworkType = registerDeviceDto.NetworkType;
                existingDevice.SignalStrength = registerDeviceDto.SignalStrength;
                existingDevice.IsOnline = true;
                existingDevice.Status = "Available";
                existingDevice.LastSeen = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await NotifyDeviceUpdate(existingDevice);

                _logger.LogInformation("Device updated: {DeviceId}", existingDevice.Id);
                return existingDevice;
            }

            var device = new Device
            {
                Id = registerDeviceDto.Id,
                Name = registerDeviceDto.Name,
                DeviceType = registerDeviceDto.DeviceType,
                Latitude = registerDeviceDto.Latitude,
                Longitude = registerDeviceDto.Longitude,
                Altitude = registerDeviceDto.Altitude,
                AdditionalInfo = registerDeviceDto.AdditionalInfo,
                BatteryLevel = registerDeviceDto.BatteryLevel,
                NetworkType = registerDeviceDto.NetworkType,
                SignalStrength = registerDeviceDto.SignalStrength,
                IsOnline = true,
                Status = "Available",
                RegisteredAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            await NotifyDeviceRegistered(device);

            _logger.LogInformation("New device registered: {DeviceId}", device.Id);
            return device;
        }

        public async Task UpdateDeviceLocationAsync(string deviceId, LocationUpdateDto locationUpdate)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Device with ID {deviceId} not found");
            }

            device.Latitude = locationUpdate.Latitude;
            device.Longitude = locationUpdate.Longitude;
            device.Altitude = locationUpdate.Altitude;
            device.LastSeen = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyLocationUpdate(device);

            _logger.LogDebug("Location updated for device: {DeviceId}", deviceId);
        }

        public async Task UpdateDeviceAvailabilityAsync(string deviceId, AvailabilityUpdateDto availabilityUpdate)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Device with ID {deviceId} not found");
            }

            device.IsOnline = availabilityUpdate.IsOnline;
            device.Status = availabilityUpdate.Status;
            device.BatteryLevel = availabilityUpdate.BatteryLevel;
            device.SignalStrength = availabilityUpdate.SignalStrength;
            device.NetworkType = availabilityUpdate.NetworkType;
            device.AdditionalInfo = availabilityUpdate.AdditionalInfo;
            device.LastSeen = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyDeviceUpdate(device);

            _logger.LogDebug("Availability updated for device: {DeviceId}", deviceId);
        }

        public async Task<IEnumerable<Device>> GetNearbyDevicesAsync(double latitude, double longitude, double radiusKm)
        {
            var devices = await _context.Devices
                .Where(d => d.IsOnline)
                .ToListAsync();

            return devices.Where(d =>
            {
                var distance = CalculateDistance(latitude, longitude, d.Latitude, d.Longitude);
                return distance <= radiusKm;
            }).OrderBy(d =>
            {
                return CalculateDistance(latitude, longitude, d.Latitude, d.Longitude);
            });
        }

        public async Task<IEnumerable<Device>> GetOnlineDevicesAsync()
        {
            return await _context.Devices
                .Where(d => d.IsOnline)
                .OrderByDescending(d => d.LastSeen)
                .ToListAsync();
        }

        public async Task<IEnumerable<Device>> GetDevicesByStatusAsync(string status)
        {
            return await _context.Devices
                .Where(d => d.Status.ToLower() == status.ToLower())
                .OrderByDescending(d => d.LastSeen)
                .ToListAsync();
        }

        public async Task<bool> RemoveDeviceAsync(string id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            await NotifyDeviceRemoved(id);

            _logger.LogInformation("Device removed: {DeviceId}", id);
            return true;
        }

        public async Task UpdateLastSeenAsync(string deviceId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device != null)
            {
                device.LastSeen = DateTime.UtcNow;
                device.IsOnline = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetOnlineDeviceCountAsync()
        {
            return await _context.Devices.CountAsync(d => d.IsOnline);
        }

        public async Task<IEnumerable<Device>> GetDevicesByTypeAsync(string deviceType)
        {
            return await _context.Devices
                .Where(d => d.DeviceType.ToLower() == deviceType.ToLower())
                .OrderByDescending(d => d.LastSeen)
                .ToListAsync();
        }

        public async Task CleanupOfflineDevicesAsync(int hoursThreshold = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursThreshold);

            var offlineDevices = await _context.Devices
                .Where(d => d.LastSeen < cutoffTime)
                .ToListAsync();

            foreach (var device in offlineDevices)
            {
                device.IsOnline = false;
                device.Status = "Offline";
            }

            if (offlineDevices.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} devices as offline", offlineDevices.Count);
            }
        }

        private async Task NotifyDeviceRegistered(Device device)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("DeviceRegistered", device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying device registration for {DeviceId}", device.Id);
            }
        }

        private async Task NotifyDeviceUpdate(Device device)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("DeviceUpdated", device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying device update for {DeviceId}", device.Id);
            }
        }

        private async Task NotifyLocationUpdate(Device device)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("LocationUpdated", new
                {
                    DeviceId = device.Id,
                    Latitude = device.Latitude,
                    Longitude = device.Longitude,
                    Altitude = device.Altitude,
                    Timestamp = device.LastSeen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying location update for {DeviceId}", device.Id);
            }
        }

        private async Task NotifyDeviceRemoved(string deviceId)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("DeviceRemoved", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying device removal for {DeviceId}", deviceId);
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