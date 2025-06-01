using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.Models
{
    public class Device
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty; // Mobile, Tablet, Desktop

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public double? Altitude { get; set; }

        [Required]
        public bool IsOnline { get; set; } = true;

        [Required]
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Available"; // Available, Busy, Emergency, Offline

        [MaxLength(500)]
        public string? AdditionalInfo { get; set; }

        public int BatteryLevel { get; set; } = 100;

        [MaxLength(50)]
        public string? NetworkType { get; set; } // Bluetooth, WiFiDirect, Radio

        public double SignalStrength { get; set; } = 0.0;

        // Navigation properties
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<MessageDelivery> ReceivedMessages { get; set; } = new List<MessageDelivery>();
        public virtual ICollection<Resource> SharedResources { get; set; } = new List<Resource>();
    }
}