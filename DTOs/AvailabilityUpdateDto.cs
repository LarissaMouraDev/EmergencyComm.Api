using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class AvailabilityUpdateDto
    {
        [Required]
        public bool IsOnline { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Available";

        [Range(0, 100)]
        public int BatteryLevel { get; set; } = 100;

        [Range(0, 100)]
        public double SignalStrength { get; set; } = 0.0;

        [MaxLength(50)]
        public string? NetworkType { get; set; }

        [MaxLength(500)]
        public string? AdditionalInfo { get; set; }
    }
}