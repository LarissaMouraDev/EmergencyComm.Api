using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class RegisterDeviceDto
    {
        [Required]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        public double? Altitude { get; set; }

        [MaxLength(500)]
        public string? AdditionalInfo { get; set; }

        [Range(0, 100)]
        public int BatteryLevel { get; set; } = 100;

        [MaxLength(50)]
        public string? NetworkType { get; set; }

        [Range(0, 100)]
        public double SignalStrength { get; set; } = 0.0;
    }
}