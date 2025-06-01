using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class LocationUpdateDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        public double? Altitude { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}