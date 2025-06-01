using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyComm.Api.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string MessageType { get; set; } = "Text"; // Text, Emergency, Broadcast, Resource, Location

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed

        public bool IsEmergency { get; set; } = false;

        public bool IsBroadcast { get; set; } = false;

        // Location data (optional)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // File attachment info (for future implementation)
        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentType { get; set; }

        // Mesh network routing
        public int HopCount { get; set; } = 0;
        public int MaxHops { get; set; } = 5;

        [MaxLength(1000)]
        public string? RoutingPath { get; set; } // JSON array of device IDs

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual Device? Sender { get; set; }

        public virtual ICollection<MessageDelivery> Deliveries { get; set; } = new List<MessageDelivery>();
    }
}