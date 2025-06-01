using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class CreateBroadcastDto
    {
        [Required]
        [MaxLength(50)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "High";

        [Required]
        public bool IsEmergency { get; set; } = true;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? RadiusKm { get; set; } = 10.0;

        public DateTime? ExpiresAt { get; set; }

        public int MaxHops { get; set; } = 10;
    }
}