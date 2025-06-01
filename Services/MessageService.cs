// MessageService.cs
using Microsoft.EntityFrameworkCore;
using EmergencyComm.Api.Data;
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;
using EmergencyComm.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyComm.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly EmergencyContext _context;
        private readonly IHubContext<CommunicationHub> _hubContext;
        private readonly ILogger<MessageService> _logger;

        public MessageService(EmergencyContext context, IHubContext<CommunicationHub> hubContext, ILogger<MessageService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Deliveries)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Deliveries)
                .ThenInclude(d => d.Recipient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Message> CreateMessageAsync(CreateMessageDto createMessageDto)
        {
            var message = new Message
            {
                SenderId = createMessageDto.SenderId,
                Content = createMessageDto.Content,
                MessageType = createMessageDto.MessageType,
                Priority = createMessageDto.Priority,
                IsEmergency = createMessageDto.IsEmergency,
                IsBroadcast = createMessageDto.IsBroadcast,
                Latitude = createMessageDto.Latitude,
                Longitude = createMessageDto.Longitude,
                ExpiresAt = createMessageDto.ExpiresAt,
                MaxHops = createMessageDto.MaxHops,
                CreatedAt = DateTime.UtcNow,
                Status = "Sent"
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (createMessageDto.RecipientIds != null && createMessageDto.RecipientIds.Any())
            {
                foreach (var recipientId in createMessageDto.RecipientIds)
                {
                    var delivery = new MessageDelivery
                    {
                        MessageId = message.Id,
                        RecipientId = recipientId,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MessageDeliveries.Add(delivery);
                }
                await _context.SaveChangesAsync();
            }

            await NotifyNewMessage(message);
            _logger.LogInformation("Message created: {MessageId} from {SenderId}", message.Id, message.SenderId);
            return message;
        }

        public async Task<Message> CreateBroadcastAsync(CreateBroadcastDto createBroadcastDto)
        {
            var message = new Message
            {
                SenderId = createBroadcastDto.SenderId,
                Content = createBroadcastDto.Content,
                MessageType = "Broadcast",
                Priority = createBroadcastDto.Priority,
                IsEmergency = createBroadcastDto.IsEmergency,
                IsBroadcast = true,
                Latitude = createBroadcastDto.Latitude,
                Longitude = createBroadcastDto.Longitude,
                ExpiresAt = createBroadcastDto.ExpiresAt,
                MaxHops = createBroadcastDto.MaxHops,
                CreatedAt = DateTime.UtcNow,
                Status = "Sent"
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (createBroadcastDto.Latitude.HasValue && createBroadcastDto.Longitude.HasValue)
            {
                var nearbyDevices = await GetNearbyDevices(
                    createBroadcastDto.Latitude.Value,
                    createBroadcastDto.Longitude.Value,
                    createBroadcastDto.RadiusKm ?? 10.0
                );

                foreach (var device in nearbyDevices.Where(d => d.Id != createBroadcastDto.SenderId))
                {
                    var delivery = new MessageDelivery
                    {
                        MessageId = message.Id,
                        RecipientId = device.Id,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MessageDeliveries.Add(delivery);
                }
                await _context.SaveChangesAsync();
            }

            await NotifyEmergencyBroadcast(message);
            _logger.LogInformation("Emergency broadcast created: {MessageId} from {SenderId}", message.Id, message.SenderId);
            return message;
        }

        public async Task UpdateMessageStatusAsync(int messageId, string status)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Message>> GetMessagesByDeviceAsync(string deviceId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.SenderId == deviceId ||
                           m.Deliveries.Any(d => d.RecipientId == deviceId))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetEmergencyMessagesAsync()
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.IsEmergency &&
                           (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetBroadcastMessagesAsync()
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.IsBroadcast &&
                           (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(int hours = 24)
        {
            var since = DateTime.UtcNow.AddHours(-hours);
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.CreatedAt >= since)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesByLocationAsync(double latitude, double longitude, double radiusKm)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Latitude.HasValue && m.Longitude.HasValue)
                .ToListAsync();

            return messages.Where(m =>
            {
                var distance = CalculateDistance(latitude, longitude, m.Latitude!.Value, m.Longitude!.Value);
                return distance <= radiusKm;
            }).OrderByDescending(m => m.CreatedAt);
        }

        public async Task<bool> MarkMessageAsDeliveredAsync(int messageId, string recipientId)
        {
            var delivery = await _context.MessageDeliveries
                .FirstOrDefaultAsync(d => d.MessageId == messageId && d.RecipientId == recipientId);

            if (delivery == null) return false;

            delivery.Status = "Delivered";
            delivery.DeliveredAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId, string recipientId)
        {
            var delivery = await _context.MessageDeliveries
                .FirstOrDefaultAsync(d => d.MessageId == messageId && d.RecipientId == recipientId);

            if (delivery == null) return false;

            delivery.Status = "Read";
            delivery.ReadAt = DateTime.UtcNow;
            if (delivery.DeliveredAt == null)
            {
                delivery.DeliveredAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MessageDelivery>> GetMessageDeliveriesAsync(int messageId)
        {
            return await _context.MessageDeliveries
                .Include(d => d.Recipient)
                .Where(d => d.MessageId == messageId)
                .ToListAsync();
        }

        private async Task<IEnumerable<Device>> GetNearbyDevices(double latitude, double longitude, double radiusKm)
        {
            var devices = await _context.Devices
                .Where(d => d.IsOnline)
                .ToListAsync();

            return devices.Where(d =>
            {
                var distance = CalculateDistance(latitude, longitude, d.Latitude, d.Longitude);
                return distance <= radiusKm;
            });
        }

        private async Task NotifyNewMessage(Message message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("NewMessage", message);

                if (message.IsEmergency)
                {
                    await _hubContext.Clients.All.SendAsync("EmergencyAlert", message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending real-time notification for message {MessageId}", message.Id);
            }
        }

        private async Task NotifyEmergencyBroadcast(Message message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("EmergencyBroadcast", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency broadcast for message {MessageId}", message.Id);
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
