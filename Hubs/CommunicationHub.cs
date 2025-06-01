using Microsoft.AspNetCore.SignalR;
using EmergencyComm.Api.Services;
using EmergencyComm.Api.DTOs;

namespace EmergencyComm.Api.Hubs
{
    public class CommunicationHub : Hub
    {
        private readonly IDeviceService _deviceService;
        private readonly IMessageService _messageService;
        private readonly ILogger<CommunicationHub> _logger;

        public CommunicationHub(
            IDeviceService deviceService,
            IMessageService messageService,
            ILogger<CommunicationHub> logger)
        {
            _deviceService = deviceService;
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (!string.IsNullOrEmpty(deviceId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
                await _deviceService.UpdateLastSeenAsync(deviceId);

                _logger.LogInformation("Device {DeviceId} connected with connection {ConnectionId}", deviceId, Context.ConnectionId);

                // Notify other clients that a device came online
                await Clients.Others.SendAsync("DeviceOnline", deviceId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (!string.IsNullOrEmpty(deviceId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Device_{deviceId}");

                _logger.LogInformation("Device {DeviceId} disconnected from connection {ConnectionId}", deviceId, Context.ConnectionId);

                // Notify other clients that a device went offline
                await Clients.Others.SendAsync("DeviceOffline", deviceId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Join a location-based group for nearby communications
        public async Task JoinLocationGroup(double latitude, double longitude, double radiusKm = 5.0)
        {
            var locationGroup = $"Location_{latitude:F3}_{longitude:F3}_{radiusKm}";
            await Groups.AddToGroupAsync(Context.ConnectionId, locationGroup);

            _logger.LogDebug("Connection {ConnectionId} joined location group {LocationGroup}", Context.ConnectionId, locationGroup);
        }

        // Leave a location-based group
        public async Task LeaveLocationGroup(double latitude, double longitude, double radiusKm = 5.0)
        {
            var locationGroup = $"Location_{latitude:F3}_{longitude:F3}_{radiusKm}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, locationGroup);

            _logger.LogDebug("Connection {ConnectionId} left location group {LocationGroup}", Context.ConnectionId, locationGroup);
        }

        // Send a direct message to a specific device
        public async Task SendDirectMessage(string recipientDeviceId, string message)
        {
            var senderDeviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(senderDeviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var createMessageDto = new CreateMessageDto
                {
                    SenderId = senderDeviceId,
                    Content = message,
                    MessageType = "DirectMessage",
                    Priority = "Normal",
                    RecipientIds = new List<string> { recipientDeviceId }
                };

                var createdMessage = await _messageService.CreateMessageAsync(createMessageDto);

                // Send to specific device group
                await Clients.Group($"Device_{recipientDeviceId}").SendAsync("DirectMessageReceived", createdMessage);

                // Confirm to sender
                await Clients.Caller.SendAsync("MessageSent", createdMessage);

                _logger.LogInformation("Direct message sent from {SenderId} to {RecipientId}", senderDeviceId, recipientDeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending direct message from {SenderId} to {RecipientId}", senderDeviceId, recipientDeviceId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        // Send an emergency broadcast
        public async Task SendEmergencyBroadcast(string message, double latitude, double longitude, double radiusKm = 10.0)
        {
            var senderDeviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(senderDeviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var createBroadcastDto = new CreateBroadcastDto
                {
                    SenderId = senderDeviceId,
                    Content = message,
                    Priority = "Critical",
                    IsEmergency = true,
                    Latitude = latitude,
                    Longitude = longitude,
                    RadiusKm = radiusKm
                };

                var broadcast = await _messageService.CreateBroadcastAsync(createBroadcastDto);

                // Send to all connected clients (emergency broadcasts go to everyone)
                await Clients.All.SendAsync("EmergencyBroadcast", broadcast);

                _logger.LogWarning("Emergency broadcast sent from {SenderId}: {Message}", senderDeviceId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency broadcast from {SenderId}", senderDeviceId);
                await Clients.Caller.SendAsync("Error", "Failed to send emergency broadcast");
            }
        }

        // Update device location
        public async Task UpdateLocation(double latitude, double longitude, double? altitude = null)
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(deviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var locationUpdate = new LocationUpdateDto
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Altitude = altitude
                };

                await _deviceService.UpdateDeviceLocationAsync(deviceId, locationUpdate);

                // Notify nearby devices of location update
                await Clients.Others.SendAsync("DeviceLocationUpdated", new
                {
                    DeviceId = deviceId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Altitude = altitude,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogDebug("Location updated for device {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for device {DeviceId}", deviceId);
                await Clients.Caller.SendAsync("Error", "Failed to update location");
            }
        }

        // Update device status
        public async Task UpdateStatus(string status, int batteryLevel = 100, double signalStrength = 0.0)
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(deviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var availabilityUpdate = new AvailabilityUpdateDto
                {
                    IsOnline = true,
                    Status = status,
                    BatteryLevel = batteryLevel,
                    SignalStrength = signalStrength
                };

                await _deviceService.UpdateDeviceAvailabilityAsync(deviceId, availabilityUpdate);

                // Notify others of status change
                await Clients.Others.SendAsync("DeviceStatusUpdated", new
                {
                    DeviceId = deviceId,
                    Status = status,
                    BatteryLevel = batteryLevel,
                    SignalStrength = signalStrength,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogDebug("Status updated for device {DeviceId}: {Status}", deviceId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for device {DeviceId}", deviceId);
                await Clients.Caller.SendAsync("Error", "Failed to update status");
            }
        }

        // Request help from nearby devices
        public async Task RequestHelp(string helpType, string description, double latitude, double longitude)
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(deviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var helpRequest = new
                {
                    DeviceId = deviceId,
                    HelpType = helpType,
                    Description = description,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = DateTime.UtcNow,
                    RequestId = Guid.NewGuid().ToString()
                };

                // Send to all devices - they can filter by proximity on client side
                await Clients.Others.SendAsync("HelpRequested", helpRequest);

                _logger.LogWarning("Help requested by device {DeviceId}: {HelpType} - {Description}", deviceId, helpType, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing help request from device {DeviceId}", deviceId);
                await Clients.Caller.SendAsync("Error", "Failed to send help request");
            }
        }

        // Respond to a help request
        public async Task RespondToHelp(string requestId, string response, bool canHelp)
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (string.IsNullOrEmpty(deviceId))
            {
                await Clients.Caller.SendAsync("Error", "Device ID not provided");
                return;
            }

            try
            {
                var helpResponse = new
                {
                    RequestId = requestId,
                    ResponderDeviceId = deviceId,
                    Response = response,
                    CanHelp = canHelp,
                    Timestamp = DateTime.UtcNow
                };

                // Send to all devices so the original requester can see responses
                await Clients.Others.SendAsync("HelpResponse", helpResponse);

                _logger.LogInformation("Help response from device {DeviceId} for request {RequestId}: {CanHelp}", deviceId, requestId, canHelp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing help response from device {DeviceId}", deviceId);
                await Clients.Caller.SendAsync("Error", "Failed to send help response");
            }
        }

        // Heartbeat to keep connection alive and update last seen
        public async Task Heartbeat()
        {
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();

            if (!string.IsNullOrEmpty(deviceId))
            {
                await _deviceService.UpdateLastSeenAsync(deviceId);
                await Clients.Caller.SendAsync("HeartbeatAck", DateTime.UtcNow);
            }
        }
    }
}