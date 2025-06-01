// IMessageService.cs
using EmergencyComm.Api.Models;
using EmergencyComm.Api.DTOs;

namespace EmergencyComm.Api.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetAllMessagesAsync();
        Task<Message?> GetMessageByIdAsync(int id);
        Task<Message> CreateMessageAsync(CreateMessageDto createMessageDto);
        Task<Message> CreateBroadcastAsync(CreateBroadcastDto createBroadcastDto);
        Task UpdateMessageStatusAsync(int messageId, string status);
        Task<bool> DeleteMessageAsync(int id);
        Task<IEnumerable<Message>> GetMessagesByDeviceAsync(string deviceId);
        Task<IEnumerable<Message>> GetEmergencyMessagesAsync();
        Task<IEnumerable<Message>> GetBroadcastMessagesAsync();
        Task<IEnumerable<Message>> GetRecentMessagesAsync(int hours = 24);
        Task<IEnumerable<Message>> GetMessagesByLocationAsync(double latitude, double longitude, double radiusKm);
        Task<bool> MarkMessageAsDeliveredAsync(int messageId, string recipientId);
        Task<bool> MarkMessageAsReadAsync(int messageId, string recipientId);
        Task<IEnumerable<MessageDelivery>> GetMessageDeliveriesAsync(int messageId);
    }
}